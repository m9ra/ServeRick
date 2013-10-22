using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using Parsing;

using SharpServer.Compiling;
using SharpServer.Languages.HAML.Compiling;

namespace SharpServer.Languages.HAML
{
    /// <summary>
    /// Compiler for HAML parsed tree
    /// </summary>
    class Compiler : CompilerBase
    {
        /// <summary>
        /// HAML Grammar for parser
        /// </summary>
        static readonly Grammar HamlGrammar = new HAML.Grammar();

        /// <summary>
        /// HAML parser
        /// </summary>
        static readonly Parser Parser = new Parser(HamlGrammar);

        private Compiler(Node root, Emitter emitter)
            : base(root, emitter)
        {
        }

        public static void Compile(string source, Emitter emitter)
        {
            source = source.Trim().Replace("\r", "").Replace("\t", "    ");

            var data = Parser.Parse(source);

            var parseOutput = Print(data);
            Console.WriteLine(parseOutput);
            if (data.Root == null)
            {
                emitter.ReportParseError(parseOutput);
                return;
            }

            var compiler = new Compiler(data.Root, emitter);
            compiler.compile();
        }

        /// <summary>
        /// Emit blocks from parsed tree
        /// </summary>
        private void compile()
        {
            E.Write(compileBlock(Root));
        }

        #region Generating methods

        private Instruction compileBlock(Node block)
        {
            var name = block.Name;
            switch (name)
            {
                case "view":
                    return compileView(block);
                case "contentBlock":
                    return compileContentBlock(block);
                case "containerBlock":
                    return compileContainerBlock(block);
                default:
                    throw new NotSupportedException("Given block is not supported");
            }
        }

        private Instruction compileView(Node view)
        {
            var declarations = GetDescendant(view, "paramDeclarations");
            if (declarations != null)
            {
                foreach (var declaration in declarations.ChildNodes)
                {
                    var name = GetTerminalText(declaration, "param", "identifier");
                    var type = GetTerminalText(declaration, "type");

                    E.DeclareParam(name, type);
                }
            }

            var blocks = compileBlocks(view);

            var doctype = GetDescendant(view, "doctype");
            if (doctype != null)
            {
                //TODO resolve doctype type
                var doctypeString = E.Constant("<!DOCTYPE html>\n");

                return E.Concat(new Instruction[] { doctypeString, blocks });
            }

            return blocks;
        }

        private Instruction compileBlocks(Node parent)
        {
            var blocks = GetDescendants(parent, "blocks", "block");

            var compiledBlocks = new List<Instruction>();
            foreach (var block in blocks)
            {
                var resolved = StepToChild(block);

                var compiledBlock = compileBlock(resolved);
                compiledBlocks.Add(compiledBlock);
            }

            return E.Concat(compiledBlocks);
        }

        private Instruction compileContentBlock(Node contentBlock)
        {
            var headNode = GetDescendant(contentBlock, "head");
            var tag = createTag(headNode);


            var contentNode = GetDescendant(contentBlock, "content");
            var content = compileContent(contentNode);
            if (tag == null)
                //empty tag declaration
                return content;

            tag.SetContent(content);
            return tag.ToInstruction();
        }

        private Instruction compileContent(Node contentNode)
        {
            if (contentNode == null)
                return null;

            var rawContent = GetTerminalText(contentNode, "rawOutput");
            if (rawContent != null)
            {
                return E.Constant(rawContent);
            }
            else
            {
                var code = GetDescendant(contentNode, "code");
                return compileCode(code);
            }
        }

        private Instruction compileCode(Node code)
        {
            var statements = GetDescendant(code, "statement");

            //TODO multiple statemets handling
            /*     Instruction lastStatement = null;
                 foreach (var statement in statements.ChildNodes)
                 {
                     lastStatement = compileStatement(statement);
                 }
            
                 return lastStatement;*/

            return compileStatement(statements);
        }

        private Instruction compileStatement(Node statement)
        {
            var child = StepToChild(statement);
            var statementName = child.Name;
            switch (statementName)
            {
                case "expression":
                    var value = resolveRValue(child);
                    var output = value.ToInstruction();

                    if (containsYield(child))
                        //yield doesn't have return value
                        return output;
                    else
                        return E.WriteInstruction(output);
                case "ifStatement":
                    var ifBranch = compileBranch(GetDescendant(child, "ifBranch"));
                    var elseBranch = compileBranch(GetDescendant(child, "elseBranch"));
                    var condition = resolveRValue(GetDescendant(child, "condition"));

                    var ifStatement = E.If(condition.ToInstruction(), ifBranch, elseBranch);
                    return ifStatement;
                default:
                    throw new NotImplementedException();
            }
        }

        private Instruction compileBranch(Node node)
        {
            return E.Constant("NotImplemented branch compilation");
        }

        private bool containsYield(Node child)
        {
            return GetDescendant(child, "yield") != null;
        }

        private Instruction compileContainerBlock(Node containerBlock)
        {
            var headNode = GetDescendant(containerBlock, "head");
            var tag = createTag(headNode);

            var blocks = compileBlocks(containerBlock);
            if (tag == null)
                return blocks;

            tag.SetContent(blocks);

            return tag.ToInstruction();
        }
        #endregion

        #region Expression resolving
        private RValue resolveRValue(Node node)
        {
            var name = node.Name;
            switch (name)
            {
                case "call":
                    return resolveCall(node);
                case "expression":
                    return resolveRValue(StepToChild(node));
                case "symbol":
                    return resolveSymbol(node);
                case "shortKey":
                    return resolveShortKey(node);
                case "value":
                    var literal = GetSubTerminalText(node);
                    return resolveLiteralValue(literal);
                case "hash":
                    return resolveHashValue(node);
                case "keyPair":
                    return resolveKeyPair(node);
                case "yield":
                    return resolveYield(node);
                case "param":
                    return resolveParam(node);
                case "condition":
                    var conditionValue = resolveRValue(StepToChild(node));
                    return resolveCondition(conditionValue);

                default:
                    throw new NotImplementedException();
            }
        }

        private RValue resolveCondition(RValue conditionValue)
        {
            return new ConditionValue(conditionValue, E);
        }

        private RValue resolveParam(Node node)
        {
            var paramName = GetSubTerminalText(node);
            return new ParamValue(paramName, E);
        }

        private RValue resolveCall(Node node)
        {
            var argValues = getArguments(node);
            var callName = GetSubTerminalText(node.ChildNodes[0]);

            return new CallValue(callName, argValues, E);
        }

        private RValue[] getArguments(Node callNode)
        {
            var argsNode = GetDescendant(callNode, "argList", "args");

            var args = new List<RValue>();

            while (argsNode != null)
            {
                var argNode = argsNode;
                if (argsNode.Name == "args")
                {
                    argNode = argsNode.ChildNodes[0];
                }
                var value = resolveRValue(argNode);
                args.Add(value);

                if (argsNode.ChildNodes.Count == 1)
                    break;

                argsNode = GetDescendant(argsNode.ChildNodes[1], "args");
            }

            return args.ToArray();
        }

        private RValue resolveSymbol(Node symbolNode)
        {
            if (symbolNode == null)
                return null;

            return new LiteralValue(GetTerminalText(symbolNode).Substring(1), E);
        }

        private RValue resolveShortKey(Node shortKeyNode)
        {
            var keyText = GetTerminalText(shortKeyNode);
            //remove ending :
            keyText = keyText.Substring(0, keyText.Length - 1);
            return new LiteralValue(keyText, E);
        }

        private RValue resolveLiteralValue(string literal)
        {
            //TODO proper literal resolving

            if (literal.StartsWith("\"") && literal.EndsWith("\""))
            {
                literal = literal.Substring(1, literal.Length - 2);
            }
            else
            {
                throw new NotImplementedException();
            }

            return new LiteralValue(literal, E);
        }

        private RValue resolveKeyPair(Node pairNode)
        {
            var key = resolveRValue(pairNode.ChildNodes[0]);
            var value = resolveRValue(pairNode.ChildNodes[1]);
            return new PairValue(key, value, E);
        }

        private RValue resolveYield(Node yieldNode)
        {
            var symbolNode = GetDescendant(yieldNode, "symbol");
            var symbol = resolveSymbol(symbolNode);

            return new YieldValue(symbol, E);
        }

        private RValue resolveHashValue(Node hashNode)
        {
            var pairs = GetDescendants(hashNode, "keyPairs", "keyPair");
            var pairValues = new List<RValue>();
            foreach (var pair in pairs)
            {
                pairValues.Add(resolveRValue(pair));
            }
            return new HashValue(pairValues, E);
        }

        #endregion

        #region Semantic translation

        private TagValue createTag(Node headNode)
        {
            if (headNode == null)
                return null;

            var hashNode = GetDescendant(headNode, "hash");
            var hash = hashNode == null ? null : resolveRValue(hashNode);
            var tag = getTagName(headNode);
            var id = getId(headNode);

            var classAttrib = getClassAttrib(headNode);

            if (tag == null && id == null && classAttrib == null)
            {
                //empty element declaration
                return null;
            }

            if (tag == null)
                //implicit tag
                tag = new LiteralValue("div", E);

            return new TagValue(tag, classAttrib, id, hash, E);
        }

        private RValue getClassAttrib(Node headNode)
        {
            var classes = GetTerminalTexts(headNode, "attributes", "class", "identifier");

            if (classes.Length == 0)
                return null;

            var classAttrib = string.Join(" ", classes);
            return new LiteralValue(classAttrib, E);
        }

        private RValue getTagName(Node headNode)
        {
            var terms = GetTerminalTexts(headNode, "tag", "identifier");
            if (terms.Length < 1)
                return null;

            return new LiteralValue(terms[0], E);
        }

        private RValue getId(Node headNode)
        {
            var terms = GetTerminalTexts(headNode, "attributes", "id", "identifier");
            if (terms.Length < 1)
                return null;

            return new LiteralValue(terms[0], E);
        }

        private Dictionary<string, RValue> parseHash(Node hashNode)
        {
            var result = new Dictionary<string, RValue>();
            if (hashNode != null)
            {
                var pairs = GetDescendants(hashNode, "keyPair");

                foreach (var pair in pairs)
                {
                    //TODO better resolving of keys
                    var symbol = GetTerminalText(pair.ChildNodes[0]);
                    var value = resolveRValue(pair.ChildNodes[1]);
                    result[symbol] = value;
                }
            }
            return result;
        }

        #endregion
    }
}
