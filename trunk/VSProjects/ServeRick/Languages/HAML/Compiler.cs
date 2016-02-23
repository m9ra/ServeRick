using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using Parsing;

using ServeRick.Compiling;
using ServeRick.Languages.HAML.Compiling;

namespace ServeRick.Languages.HAML
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

        private readonly Stack<Context> _contextStack = new Stack<Context>();

        private Context CurrentContext { get { return _contextStack.Peek(); } }

        static readonly Dictionary<string, string> TypeTranslations = new Dictionary<string, string>()
        {
            {"string","System.String"},
            {"int","System.Int32"},
            {"bool","System.Boolean"}
        };

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
            pushContext();
            E.Emit(compileBlock(Root));
            popContext();
            if (_contextStack.Count != 0)
            {
                throw new NotSupportedException("Incorrect context stack handling");
            }
        }

        #region Context stack handling

        /// <summary>
        /// Push context to stack, with respect to previous contexts
        /// </summary>
        /// <returns>Pushed context</returns>
        private Context pushContext()
        {
            Context pushedContext;
            if (_contextStack.Count == 0)
            {
                pushedContext = new Context(E);
            }
            else
            {
                pushedContext = CurrentContext.CreateSubContext();
            }

            _contextStack.Push(pushedContext);
            return pushedContext;
        }

        /// <summary>
        /// Pop context from stack
        /// </summary>
        /// <returns>Popped stack</returns>
        private Context popContext()
        {
            return _contextStack.Pop();
        }

        #endregion

        /// <summary>
        /// Declara parameters from declaratoins node
        /// </summary>
        /// <param name="declarations">Node with declarations</param>
        private void declareParameters(Node declarations)
        {
            if (declarations != null)
            {
                foreach (var declaration in declarations.ChildNodes)
                {
                    var name = GetTerminalText(declaration, "param", "identifier");
                    var modifier = GetTerminalText(declaration, "typeModifier");
                    var type = GetTerminalText(declaration, "type");


                    if (TypeTranslations.ContainsKey(type))
                        type = TypeTranslations[type];


                    switch (modifier)
                    {
                        case "+":
                            type = string.Format("System.Collections.Generic.IEnumerable`1[{0}]", type);
                            break;
                    }

                    CurrentContext.DeclareParam(name, type);
                }
            }
        }

        #region Blocks compilation

        /// <summary>
        /// Compile whole view
        /// </summary>
        /// <param name="view">View node to be compiled</param>
        /// <returns>Compiled view instruction</returns>
        private Instruction compileView(Node view)
        {
            var declarations = GetDescendant(view, "paramDeclarations");
            declareParameters(declarations);

            var blocksNode = GetDescendant(view, "blocks");
            var blocks = compileBlocks(blocksNode);

            var doctype = GetDescendant(view, "doctype");
            if (doctype != null)
            {
                //TODO resolve doctype type
                var doctypeString = E.WriteInstruction(E.Constant("<!DOCTYPE html>\n"));

                return E.Sequence(new Instruction[] { doctypeString, blocks });
            }

            return blocks;
        }

        /// <summary>
        /// Compile blocks from given node. Blocks are chained into sequence 
        /// instruction.
        /// </summary>
        /// <param name="blocksNode">Node with blocks</param>
        /// <returns>Sequence of compiled blocks</returns>
        private Instruction compileBlocks(Node blocksNode)
        {
            var blocks = GetDescendants(blocksNode, "block");

            var compiledBlocks = new List<Instruction>();
            foreach (var block in blocks)
            {
                var resolved = StepToChild(block);

                var compiledBlock = compileBlock(resolved);
                if (compiledBlock == null)
                    continue;

                compiledBlocks.Add(compiledBlock);
            }

            return E.Sequence(compiledBlocks);
        }

        /// <summary>
        /// Compile block from given node
        /// <remarks>Block defines output on it's own</remarks>
        /// </summary>
        /// <param name="block">Block to be compiled</param>
        /// <returns>Compiled block</returns>
        private Instruction compileBlock(Node block)
        {
            var name = block.Name;
            switch (name)
            {
                case "view":
                    return compileView(block);
                case "contentBlock":
                    return compileContentBlock(block);
                default:
                    throw new NotSupportedException("Given block is not supported");
            }
        }

        /// <summary>
        /// Compile block with content
        /// </summary>
        /// <param name="contentBlock">Block to be compiled</param>
        /// <returns>Compiled block</returns>
        private Instruction compileContentBlock(Node contentBlock)
        {
            var headNode = GetDescendant(contentBlock, "head");
            var tag = createTag(headNode);


            var blocksNode = GetDescendant(contentBlock, "blocks");
            var contentNode = GetDescendant(contentBlock, "content");
            Instruction content;

            if (blocksNode != null)
                content = compileBlocks(blocksNode);
            else
                content = compileContent(contentNode);

            if (tag == null)
                //empty tag declaration
                return content;

            tag.SetContent(content);
            return tag.ToInstruction();
        }

        #endregion

        #region Content compilation

        private Instruction compileContent(Node contentNode)
        {
            if (contentNode == null)
                return null;

            Instruction compiled;

            var rawContent = GetTerminalText(contentNode, "rawOutput");
            if (rawContent == null)
            {
                var code = GetDescendant(contentNode, "code");
                compiled = compileCode(code);
            }
            else
            {
                //rawContent is always written to output
                compiled = E.WriteInstruction(E.Constant(rawContent));
            }

            return compiled;
        }

        #endregion

        #region Code compilation

        private Instruction compileCode(Node code)
        {
            if (code == null)
                return null;

            var statements = GetDescendant(code, "statement");

            //TODO multiple statemets handling
            /*     Instruction lastStatement = null;
                 foreach (var statement in statements.ChildNodes)
                 {
                     lastStatement = compileStatement(statement);
                 }
            
                 return lastStatement;*/

            var prefix = GetTerminalText(code.ChildNodes[0]);
            var isRaw = GetTerminalText(code.ChildNodes[1]) == "raw";

            var compiled = compileStatement(statements);
            if (compiled.ReturnType == typeof(void))
            {
                //TODO correct handling of yield
                //TODO void statements should return "0"
                return compiled;
            }

            switch (prefix)
            {
                case "=":
                    var escaped = isRaw ? compiled : escapeHTML(compiled);
                    compiled = E.WriteInstruction(escaped);
                    break;
                case "-":
                    //silent code
                    break;
                default:
                    throw new NotImplementedException();
            }

            return compiled;
        }

        private Instruction escapeHTML(Instruction compiled)
        {
            var escaped = E.Call("EscapeHTML", new Instruction[] { compiled });

            return escaped;
        }

        private Instruction compileStatement(Node statement)
        {
            var child = StepToChild(statement);
            var statementName = child.Name;

            Instruction compiled;
            switch (statementName)
            {
                case "expression":
                    var value = resolveRValue(child);
                    compiled = value.ToInstruction();

                    break;
                case "ifStatement":
                    var ifBranch = compileBranch(GetDescendant(child, "ifBranch"));
                    var elseBranch = compileBranch(GetDescendant(child, "elseBranch"));
                    var condition = resolveRValue(GetDescendant(child, "condition"));

                    var ifStatement = E.If(condition.ToInstruction(), ifBranch, elseBranch);
                    return ifStatement;
                default:
                    throw new NotImplementedException();
            }

            return compiled;
        }

        private Instruction compileBranch(Node node)
        {
            if (node == null)
                return null;

            var blocks = GetDescendant(node, "branch", "blocks");
            if (blocks == null)
            {
                var statement = GetDescendant(node, "branch", "statement");
                return compileStatement(statement);
            }
            else
            {
                return compileBlocks(blocks);
            }
        }

        private bool containsYield(Node child)
        {
            return GetDescendant(child, "yield") != null;
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
                case "binaryExpression":
                    return resolveBinary(node);
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
                case "interval":
                    var fromVal = resolveRValue(node.ChildNodes[0]);
                    var toVal = resolveRValue(node.ChildNodes[1]);
                    return resolveInterval(fromVal, toVal);
                case "condition":
                    var conditionValue = resolveRValue(StepToChild(node));
                    return resolveCondition(conditionValue);
                case "methodCall":
                    var calledObjNode = GetDescendant(node, "calledObject");
                    var calledObj = resolveRValue(StepToChild(calledObjNode));

                    var callNode = GetDescendant(node, "call");
                    return resolveMethodCall(calledObj, callNode);
                case "identifier":
                    var identifier = GetTerminalText(node);
                    var variable = VariableValue.TryGet(identifier, CurrentContext);
                    if (variable != null)
                        return variable;

                    return new CallValue(identifier, new RValue[0], CurrentContext);
                default:
                    throw new NotImplementedException();
            }
        }

        private RValue resolveInterval(RValue fromVal, RValue toVal)
        {
            return new IntervalValue(fromVal, toVal, CurrentContext);
        }

        private RValue resolveMethodCall(RValue calledObj, string callName, IEnumerable<RValue> arguments = null)
        {
            if (arguments == null)
                arguments = new RValue[0];

            return new MethodCallValue(calledObj, callName, arguments.ToArray(), CurrentContext);
        }

        private RValue resolveMethodCall(RValue calledObj, Node callNode)
        {
            //TODO create signature based support for lambdaBlocks
            var callName = GetTerminalText(callNode, "callName", "identifier");
            switch (callName)
            {
                case "each":
                    var lambdaBlockNode = GetDescendant(callNode, "lambdaBlock");

                    var enumeratedItemType = calledObj.ReturnType().GetGenericArguments()[0];
                    var lambdaBlock = resolveLambdaBlock(lambdaBlockNode, enumeratedItemType);
                    return new ForeachValue(calledObj, lambdaBlock, CurrentContext);

                default:
                    var args = getArguments(callNode);
                    return resolveMethodCall(calledObj, callName, args);
            }
        }

        private LambdaBlock resolveLambdaBlock(Node lambdaBlockNode, params Type[] inputArgumentTypes)
        {
            var blockParameters = GetTerminalTexts(lambdaBlockNode, "blockArguments", "identifier");
            if (blockParameters.Length != inputArgumentTypes.Length)
                throw new NotSupportedException("Invalid lambda parameters count");

            var lambdaContext = pushContext();
            var declaredParameters = new List<VariableInstruction>();
            for (int i = 0; i < blockParameters.Length; ++i)
            {
                var argType = inputArgumentTypes[i];
                var parameterName = blockParameters[i];
                var declaredParameter = lambdaContext.DeclareVariable(parameterName, argType);

                declaredParameters.Add(declaredParameter);
            }

            var blocksNode = GetDescendant(lambdaBlockNode, "blocks");
            var blocks = compileBlocks(blocksNode);

            popContext();

            return new LambdaBlock(blocks, declaredParameters, lambdaContext);
        }

        private RValue resolveCondition(RValue conditionValue)
        {
            return new ConditionValue(conditionValue, CurrentContext);
        }

        private RValue resolveParam(Node node)
        {
            var paramName = GetSubTerminalText(node);
            return new ParamValue(paramName, CurrentContext);
        }

        private RValue resolveCall(Node node)
        {
            var argValues = getArguments(node);
            var callName = GetSubTerminalText(node.ChildNodes[0]);

            return new CallValue(callName, argValues, CurrentContext);
        }

        private RValue resolveBinary(Node node)
        {
            var leftOperand = resolveRValue(node.ChildNodes[0]);
            var rightOperand = resolveRValue(node.ChildNodes[2]);
            var binaryOperator = node.ChildNodes[1].Match.MatchedData;

            string staticOperatorMethod = null;
            switch (binaryOperator)
            {
                case "==":
                    staticOperatorMethod = "equals";
                    break;

                case "+":
                    staticOperatorMethod = "concat";
                    break;

                default:
                    throw new NotImplementedException("Support for operator '" + binaryOperator + "' is not implemented yet");
            }

            return new CallValue(staticOperatorMethod, new[] { leftOperand, rightOperand }, CurrentContext);
        }

        private RValue[] getArguments(Node callNode)
        {
            var argNodes = GetDescendant(callNode, "argList", "args");
            if (argNodes == null)
                return new RValue[0];

            var args = new List<RValue>();
            foreach (var argNode in argNodes.ChildNodes)
            {
                var arg = resolveRValue(argNode);
                args.Add(arg);
            }

            return args.ToArray();
        }

        private RValue resolveSymbol(Node symbolNode)
        {
            if (symbolNode == null)
                return null;

            return new LiteralValue(GetTerminalText(symbolNode).Substring(1), CurrentContext);
        }

        private RValue resolveShortKey(Node shortKeyNode)
        {
            var keyText = GetTerminalText(shortKeyNode);
            //remove ending :
            keyText = keyText.Substring(0, keyText.Length - 1);
            return new LiteralValue(keyText, CurrentContext);
        }

        private RValue resolveLiteralValue(string literal)
        {
            //TODO proper literal resolving

            object literalValue;
            int number;
            bool boolean;

            if (literal.StartsWith("\"") && literal.EndsWith("\""))
            {
                literalValue = literal.Substring(1, literal.Length - 2).Replace("\\n", "\n");
            }
            else if (int.TryParse(literal, out number))
            {
                literalValue = number;
            }
            else if (bool.TryParse(literal, out boolean))
            {
                literalValue = boolean;
            }
            else
            {
                throw new NotImplementedException();
            }

            return new LiteralValue(literalValue, CurrentContext);
        }

        private RValue resolveKeyPair(Node pairNode)
        {
            var key = resolveRValue(pairNode.ChildNodes[0]);
            var value = resolveRValue(pairNode.ChildNodes[1]);
            return new PairValue(key, value, CurrentContext);
        }

        private RValue resolveYield(Node yieldNode)
        {
            var symbolNode = GetDescendant(yieldNode, "symbol");
            var symbol = resolveSymbol(symbolNode);

            return new YieldValue(symbol, CurrentContext);
        }

        private RValue resolveHashValue(Node hashNode)
        {
            var pairs = GetDescendants(hashNode, "keyPairs", "keyPair");
            var pairValues = new List<RValue>();
            foreach (var pair in pairs)
            {
                pairValues.Add(resolveRValue(pair));
            }
            return new HashValue(pairValues, CurrentContext);
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
                tag = new LiteralValue("div", CurrentContext);

            return new TagValue(tag, classAttrib, id, hash, CurrentContext);
        }

        private RValue getClassAttrib(Node headNode)
        {
            var classes = GetTerminalTexts(headNode, "attributes", "class", "identifier");

            if (classes.Length == 0)
                return null;

            var classAttrib = string.Join(" ", classes);
            return new LiteralValue(classAttrib, CurrentContext);
        }

        private RValue getTagName(Node headNode)
        {
            var terms = GetTerminalTexts(headNode, "tag", "identifier");
            if (terms.Length < 1)
                return null;

            return new LiteralValue(terms[0], CurrentContext);
        }

        private RValue getId(Node headNode)
        {
            var terms = GetTerminalTexts(headNode, "attributes", "id", "identifier");
            if (terms.Length < 1)
                return null;

            return new LiteralValue(terms[0], CurrentContext);
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
