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

        private Compiler(ParseTreeNode root,Emitter emitter)
        {
            E = emitter;
            _root = root;
        }

        public static void Compile(ParseTreeNode root,Emitter emitter)
        {
            var compiler = new Compiler(root,emitter);
            compiler.compile();
        }


        private void compile()
        {            
            generateBlock(_root);
        }

        #region Generating methods
        private void generateBlock(ParseTreeNode block)
        {
            var name=getName(block);
            switch (name)
            {
                case "view":
                    generateView(block);
                    break;
                case "contentBlock":
                    generateContentBlock(block);
                    break;
                case "containerBlock":
                    generateContainerBlock(block);
                    break;
                default:
                    throw new NotSupportedException("Given block is not supported");
            }
        }

        private void generateView(ParseTreeNode view)
        {            
            generateBlocks(view);
        }

        private void generateBlocks(ParseTreeNode parent)
        {
            var blocks = getChilds("blocks.block", parent);
            foreach (var block in blocks)
            {
                var resolved = stepToChild(block);

                generateBlock(resolved);
            }
        }

        private void generateContentBlock(ParseTreeNode contentBlock)
        {
            var headNode=getChild("head",contentBlock);
            var head = createHead(headNode);

            var contentNode=getChild("content",contentBlock);
            
            E.StaticWrite(head.OpeningTag);
            generateContent(contentNode);
            E.StaticWrite(head.ClosingTag);
        }

        private void generateContent(ParseTreeNode contentNode)
        {
            var rawContent = getSubTerminal(contentNode);
            if (rawContent != null){
                E.StaticWrite(rawContent);
            }else{
                var code = getChild("code", contentNode);
                generateCode(code);
            }
        }

        private void generateCode(ParseTreeNode code)
        {
            var statement=getChild("statement",code);
            generateStatement(statement);
        }

        private void generateStatement(ParseTreeNode statement)
        {
            var child = stepToChild(statement);
            var statementName=getName(child);
            switch (statementName)
            {
                case "expression":                    
                    var value = resolveRValue(child);
                    var output=value.ToExpression();
                    E.Write(output);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void generateContainerBlock(ParseTreeNode containerBlock)
        {
            var headNode = getChild("head", containerBlock);
            var head = createHead(headNode);

            E.StaticWrite(head.OpeningTag);
            generateBlocks(containerBlock);
            E.StaticWrite(head.ClosingTag);
        }
        #endregion

        #region Expression resolving
        private RValue resolveRValue(ParseTreeNode node)
        {
            var name = getName(node);
            switch (name)
            {
                case "call":
                    var argValues=getArguments(node);

                    var callName=getSubTerminal(node,null,"callName");
                    return new CallValue(callName, argValues,E);

                case "expression":
                    return resolveRValue(stepToChild(node));
                case "value":
                    var literal=getSubTerminal(node);
                    return resolveLiteralValue(literal);

                default:
                    throw new NotImplementedException();
            }
        }

        private RValue[] getArguments(ParseTreeNode callNode)
        {
            var argsNode = findNode(callNode, "argList", "args");

            var args = new List<RValue>();

            foreach (var argNode in argsNode.ChildNodes)
            {
                var value = resolveRValue(argNode);
                args.Add(value);
            }

            return args.ToArray();
        }

        private RValue resolveLiteralValue(string literal)
        {
            return new LiteralValue(literal,E); 
        }


        #endregion


        #region Semantic translation

        private HeadInfo createHead(ParseTreeNode headNode)
        {
            var tag=getTerminals("tag",headNode,"")[0];            
            var id=findTerminals("id",headNode,"")[0];
            var classes = findTerminals("class", headNode);

            return new HeadInfo(tag, id, classes);
        }    

        #endregion

        #region AST utilities

        private string getName(ParseTreeNode node)
        {
            return node.Term.Name;
        }

        private ParseTreeNode getChild(string name,ParseTreeNode parent)
        {
            foreach (var child in parent.ChildNodes)
            {
                var named = skipUnnamedChildren(child);
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
            if (parent.ChildNodes.Count != 1)
            {
                throw new NotSupportedException("Cannot step to child, invalid child count");
            }

            return skipUnnamedChildren(parent.ChildNodes[0]);
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
            var node = findNode(parent,path.ToArray());

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
                if (node.ChildNodes.Count != 1)
                    throw new NotSupportedException("Unsupported node");
                node = node.ChildNodes[0];
            }

            return node;
        }

        private ParseTreeNode findNode(ParseTreeNode root,params string[] pathParts)
        {
            var node = root;
            foreach(var pathPart in pathParts)
            {
                node = skipUnnamedChildren(node);

                bool found=false;
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
            var nodes=new Stack<ParseTreeNode>();
            nodes.Push(parent);

            var terminals = new List<string>();
            while (nodes.Count > 0)
            {
                var node=nodes.Pop();
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

        private string getSubTerminal(ParseTreeNode node,string defaultValue=null,params string[] pathParts)
        {
            node = findNode(node, pathParts);

            if(node.ChildNodes.Count!=1){
                return defaultValue;
            }
            var termNode = node.ChildNodes[0];
            if (termNode.Token == null)
                //is not terminal
                return defaultValue;
            return termNode.Token.Text;
        }

        private string[] getTerminals(string name, ParseTreeNode parent,string defaultValue=null)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            var nodes = getChilds(name, parent);
            var terminals = (from node in nodes select getSubTerminal(node)).ToArray();

            if (terminals.Length == 0 && defaultValue!=null)
            {
                return new string[] { defaultValue };
            }

            return terminals;
        }
        #endregion

    }
}
