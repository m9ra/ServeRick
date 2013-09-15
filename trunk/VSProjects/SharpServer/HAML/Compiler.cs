using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

using SharpServer.Compiling;
using SharpServer.HAML.Compiling;

namespace SharpServer.HAML
{
    class Compiler
    {
        readonly ParseTreeNode _root;
        readonly Stack<Emitter> _stack = new Stack<Emitter>();
        readonly Emitter E;

        private Compiler(ParseTreeNode root, Emitter emitter)
        {
            E = emitter;
            _root = root;
        }

        public static void Compile(ParseTreeNode root, Emitter emitter)
        {
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
            return compileBlocks(view);
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
            if (contentNode != null)
            {
                var content = compileContent(contentNode);
                tag.SetContent(content);
            }

            return tag.ToInstruction();
        }

        private Instruction compileContent(ParseTreeNode contentNode)
        {
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
            var statement = getChild("statement", code);
            return compileStatement(statement);
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
                    return output;
                default:
                    throw new NotImplementedException();
            }
        }

        private Instruction compileContainerBlock(ParseTreeNode containerBlock)
        {
            var headNode = getChild("head", containerBlock);
            var tag = createTag(headNode);

            var blocks = compileBlocks(containerBlock);
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
                case "value":
                    var literal = getSubTerminal(node);
                    return resolveLiteralValue(literal);
                case "hash":
                    return resolveHashValue(node);
                case "keyPair":
                    return resolveKeyPair(node);

                default:
                    throw new NotImplementedException();
            }
        }

        private RValue[] getArguments(ParseTreeNode callNode)
        {
            var argsNode = getNode(callNode, "argList", "args");

            var args = new List<RValue>();

            foreach (var argNode in argsNode.ChildNodes)
            {
                var value = resolveRValue(argNode);
                args.Add(value);
            }

            return args.ToArray();
        }

        private RValue resolveSymbol(ParseTreeNode symbolNode)
        {
            return new LiteralValue(getTerminalText(symbolNode).Substring(1), E);
        }

        private RValue resolveLiteralValue(string literal)
        {
            //TODO proper literal resolving

            if (literal.Contains("\""))
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

            return new TagValue(tag, classAttrib, id, hash, E);
        }

        private RValue getClassAttrib(ParseTreeNode headNode)
        {
            var classes = findTerminals("class", headNode);
            var classAttrib = string.Join(" ", classes);

            return new LiteralValue(classAttrib, E);
        }

        private RValue getTagName(ParseTreeNode headNode)
        {
            string tag=null;
            if (headNode.ChildNodes.Count != 0)
            {
                var tagNode = headNode.ChildNodes[0];
                tag = getTerminalText(tagNode.ChildNodes[0]);
            }
          

            if (tag == null)
            {
                tag = "div";
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
                    var symbol = getTerminalText(pair.ChildNodes[0]);
                    var value = resolveRValue(pair.ChildNodes[1]);
                    result[symbol] = value;
                }
            }
            return result;
        }

        #endregion

        #region AST utilities

        private string getName(ParseTreeNode node)
        {
            return node.Term.Name;
        }

        private ParseTreeNode getChild(string name, ParseTreeNode parent)
        {
            foreach (var child in parent.ChildNodes)
            {
                var named = skipUnnamedChildren(child);
                if (named == null)
                    break;

                if (getName(named) == name)
                    return named;
            }

            return null;
        }

        /// <summary>
        /// Step to single child of parent. Skips unnamed nodes
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private ParseTreeNode stepToChild(ParseTreeNode parent)
        {
            var count = parent.ChildNodes.Count;
            switch (count)
            {
                case 0:
                    return null;
                case 1:
                    return skipUnnamedChildren(parent.ChildNodes[0]);
                default:
                    throw new NotSupportedException("Cannot step to child, invalid child count");
            }
        }



        private string getChildName(ParseTreeNode parent)
        {
            var child = stepToChild(parent);

            return getName(child);
        }

        private IEnumerable<ParseTreeNode> getChilds(string selector, ParseTreeNode parent)
        {
            var pathParts = selector.Split('.');
            var targetName = pathParts.Last();
            var path = pathParts.Take(pathParts.Length - 1);

            //get node where children will be searched
            var node = getNode(parent, path.ToArray());

            node = skipUnnamedChildren(node);

            var result = new List<ParseTreeNode>();

            foreach (var child in node.ChildNodes)
            {
                if (getName(child) == targetName)
                {
                    result.Add(child);
                }
            }

            return result;
        }

        private ParseTreeNode skipUnnamedChildren(ParseTreeNode node)
        {
            while (!hasName(node))
            {
                var count = node.ChildNodes.Count;
                switch (count)
                {
                    case 0:
                        return null;
                    case 1:
                        node = node.ChildNodes[0];
                        break;
                    default:
                        throw new NotSupportedException("Unsupported node");
                }
            }

            return node;
        }

        private ParseTreeNode findNode(ParseTreeNode root, string name)
        {
            var nodes = findNodes(root, name);
            if (nodes.Any())
                return nodes.First();

            return null;
        }

        private IEnumerable<ParseTreeNode> findNodes(ParseTreeNode root, string name)
        {
            var queue = new Queue<ParseTreeNode>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                node = skipUnnamedChildren(node);
                if (node == null)
                {
                    continue;
                }

                if (getName(node) == name)
                    yield return node;

                foreach (var child in node.ChildNodes)
                    queue.Enqueue(child);
            }
        }

        private ParseTreeNode getNode(ParseTreeNode root, params string[] pathParts)
        {
            var node = root;
            foreach (var pathPart in pathParts)
            {
                node = skipUnnamedChildren(node);

                bool found = false;
                foreach (var child in node.ChildNodes)
                {
                    var name = getName(skipUnnamedChildren(child));

                    if (name == pathPart)
                    {
                        node = child;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new KeyNotFoundException("Node hasn't been found");
                }
            }
            return node;
        }

        private bool hasName(ParseTreeNode node)
        {
            return !getName(node).StartsWith("Unnamed");
        }

        private string[] findTerminals(string name, ParseTreeNode parent, string defaultValue = null)
        {
            var nodes = new Stack<ParseTreeNode>();
            nodes.Push(parent);

            var terminals = new List<string>();
            while (nodes.Count > 0)
            {
                var node = nodes.Pop();
                foreach (var child in node.ChildNodes)
                {
                    nodes.Push(child);
                }

                if (getName(node) == name)
                {
                    terminals.Add(node.ChildNodes[0].Token.Text);
                }
            }

            if (terminals.Count == 0 && defaultValue != null)
            {
                terminals.Add(defaultValue);
            }

            terminals.Reverse();
            return terminals.ToArray();
        }

        private string getTerminalText(ParseTreeNode terminal)
        {
            if (terminal.Token == null)
            {
                return null;
            }
            return terminal.Token.Text;
        }

        private string getSubTerminal(ParseTreeNode node, string defaultValue = null, params string[] pathParts)
        {
            node = getNode(node, pathParts);

            if (node.ChildNodes.Count != 1)
            {
                return defaultValue;
            }
            var termNode = node.ChildNodes[0];
            if (termNode.Token == null)
                //is not terminal
                return defaultValue;
            return getTerminalText(termNode);
        }

        private string[] getTerminals(string name, ParseTreeNode parent, string defaultValue = null)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            var nodes = getChilds(name, parent);
            var terminals = (from node in nodes select getSubTerminal(node)).ToArray();

            if (terminals.Length == 0 && defaultValue != null)
            {
                return new string[] { defaultValue };
            }

            return terminals;
        }
        #endregion

    }
}
