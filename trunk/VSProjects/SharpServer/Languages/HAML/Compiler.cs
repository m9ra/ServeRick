using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

using SharpServer.Compiling;
using SharpServer.Languages.HAML.Compiling;

namespace SharpServer.Languages.HAML
{
    class Compiler : CompilerBase
    {
        static readonly Grammar HamlGrammar = new HAML.Grammar();
        static readonly LanguageData HamlLang = new LanguageData(HamlGrammar);
        static readonly Parser Parser = new Parser(HamlLang);

        readonly ParseTreeNode _root;
        readonly Emitter E;

        private Compiler(ParseTreeNode root, Emitter emitter)
        {
            E = emitter;
            _root = root;
        }

        public static void Compile(string source, Emitter emitter)
        {
            var tree = Parser.Parse(source);
            var root = tree.Root;

            var parseOutput = Output.AsString(tree);
            if (root == null)
            {
                emitter.ReportParseError(parseOutput);
                return;
            }

            Console.WriteLine(parseOutput);

            var compiler = new Compiler(root, emitter);
            compiler.compile();
        }

        private void compile()
        {
            E.Write(compileBlock(_root));
        }

        #region Generating methods
        private Instruction compileBlock(ParseTreeNode block)
        {
            var name = getName(block);
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

        private Instruction compileView(ParseTreeNode view)
        {
            var blocks = compileBlocks(view);

            var doctype = findNode(view, "doctype");
            if (doctype != null)
            {
                //TODO resolve doctype type
                var doctypeString = E.Constant("<!DOCTYPE html>\n");

                return E.Concat(new Instruction[] { doctypeString, blocks });
            }
            return blocks;
        }

        private Instruction compileBlocks(ParseTreeNode parent)
        {
            var blocks = getChilds("blocks.block", parent);

            var compiledBlocks = new List<Instruction>();
            foreach (var block in blocks)
            {
                var resolved = stepToChild(block);

                var compiledBlock = compileBlock(resolved);
                compiledBlocks.Add(compiledBlock);
            }

            return E.Concat(compiledBlocks);
        }

        private Instruction compileContentBlock(ParseTreeNode contentBlock)
        {
            var headNode = getChild("head", contentBlock);
            var tag = createTag(headNode);


            var contentNode = getChild("content", contentBlock);
            var content = compileContent(contentNode);
            if (tag == null)
                //empty tag declaration
                return content;

            tag.SetContent(content);
            return tag.ToInstruction();
        }

        private Instruction compileContent(ParseTreeNode contentNode)
        {
            if (contentNode == null)
                return null;
            var rawContent = getSubTerminal(contentNode);
            if (rawContent != null)
            {
                return E.Constant(rawContent);
            }
            else
            {
                var code = getChild("code", contentNode);
                return compileCode(code);
            }
        }

        private Instruction compileCode(ParseTreeNode code)
        {
            var statements = getChild("statement", code);

            //TODO multiple statemets handling
       /*     Instruction lastStatement = null;
            foreach (var statement in statements.ChildNodes)
            {
                lastStatement = compileStatement(statement);
            }
            
            return lastStatement;*/

            return compileStatement(statements);
        }

        private Instruction compileStatement(ParseTreeNode statement)
        {
            var child = stepToChild(statement);
            var statementName = getName(child);
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
                default:
                    throw new NotImplementedException();
            }
        }

        private bool containsYield(ParseTreeNode child)
        {
            return findNode(child, "yield") != null;
        }



        private Instruction compileContainerBlock(ParseTreeNode containerBlock)
        {
            var headNode = getChild("head", containerBlock);
            var tag = createTag(headNode);

            var blocks = compileBlocks(containerBlock);
            if (tag == null)
                return blocks;

            tag.SetContent(blocks);

            return tag.ToInstruction();
        }
        #endregion

        #region Expression resolving
        private RValue resolveRValue(ParseTreeNode node)
        {
            var name = getName(node);
            switch (name)
            {
                case "call":
                    var argValues = getArguments(node);
                    var callName = getSubTerminal(node, null, "callName");
                    return new CallValue(callName, argValues, E);
                case "expression":
                    return resolveRValue(stepToChild(node));
                case "symbol":
                    return resolveSymbol(node);
                case "shortKey":
                    return resolveShortKey(node);
                case "value":
                    var literal = getSubTerminal(node);
                    return resolveLiteralValue(literal);
                case "hash":
                    return resolveHashValue(node);
                case "keyPair":
                    return resolveKeyPair(node);
                case "yield":
                    return resolveYield(node);

                default:
                    throw new NotImplementedException();
            }
        }

        private RValue[] getArguments(ParseTreeNode callNode)
        {
            var argsNode = GetNode(callNode, "argList", "args");

            var args = new List<RValue>();

            while (argsNode != null)
            {
                var argNode = argsNode;
                if (argsNode.Term.Name == "args")
                {
                    argNode = argsNode.ChildNodes[0];
                }
                var value = resolveRValue(argNode);
                args.Add(value);

                if (argsNode.ChildNodes.Count == 1)
                    break;

                argsNode = findNode(argsNode.ChildNodes[1], "args");
            }

            return args.ToArray();
        }

        private RValue resolveSymbol(ParseTreeNode symbolNode)
        {
            if (symbolNode == null)
                return null;

            return new LiteralValue(GetTerminalText(symbolNode).Substring(1), E);
        }

        private RValue resolveShortKey(ParseTreeNode shortKeyNode)
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

        private RValue resolveKeyPair(ParseTreeNode pairNode)
        {
            var key = resolveRValue(pairNode.ChildNodes[0]);
            var value = resolveRValue(pairNode.ChildNodes[1]);
            return new PairValue(key, value, E);
        }

        private RValue resolveYield(ParseTreeNode yieldNode)
        {
            var symbolNode = findNode(yieldNode, "symbol");
            var symbol = resolveSymbol(symbolNode);

            return new YieldValue(symbol, E);
        }

        private RValue resolveHashValue(ParseTreeNode hashNode)
        {
            var pairs = findNodes(hashNode, "keyPair").ToArray();
            var pairValues = new List<RValue>();
            foreach (var pair in pairs)
            {
                pairValues.Add(resolveRValue(pair));
            }
            return new HashValue(pairValues, E);
        }

        #endregion

        #region Semantic translation

        private TagValue createTag(ParseTreeNode headNode)
        {
            var hashNode = findNode(headNode, "hash");
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

        private RValue getClassAttrib(ParseTreeNode headNode)
        {
            var classes = findTerminals("class", headNode);

            if (classes.Length == 0)
                return null;

            var classAttrib = string.Join(" ", classes);
            return new LiteralValue(classAttrib, E);
        }

        private RValue getTagName(ParseTreeNode headNode)
        {
            string tag = null;
            if (headNode.ChildNodes.Count != 0)
            {
                var tagNode = headNode.ChildNodes[0];
                tag = GetTerminalText(tagNode.ChildNodes[0]);
            }

            if (tag == null)
            {
                return null;
            }

            return new LiteralValue(tag, E);
        }

        private RValue getId(ParseTreeNode headNode)
        {
            var terms = findTerminals("id", headNode);
            if (terms.Length < 1)
                return null;

            return new LiteralValue(terms[0], E);
        }

        private Dictionary<string, RValue> parseHash(ParseTreeNode hashNode)
        {
            var result = new Dictionary<string, RValue>();
            if (hashNode != null)
            {
                var pairs = findNodes(hashNode, "keyPair");

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
