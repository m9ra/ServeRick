using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing;
using Irony.Parsing;

namespace SharpServer.Compiling
{
    class CompilerBase
    {

        #region AST utilities

        /// <summary>
        /// Print node representation to console
        /// </summary>
        /// <param name="node">Root node of printed tree</param>
        /// <param name="level">Level of indentation</param>
        static protected void Print(Node node, int level = 0)
        {
            Console.WriteLine("".PadLeft(level * 2, ' ') + node);
            foreach (var child in node.ChildNodes)
            {
                Print(child, level + 1);
            }
        }

        /// <summary>
        /// Get first node, found from root, through given path
        /// </summary>
        /// <param name="root">Root node, where searching starts</param>
        /// <param name="path">Path specifiing names of child hierarchy</param>
        /// <returns>Founded node, or null if no suitable node is found</returns>
        protected Node GetDescendant(Node root, params string[] path)
        {
            if (path.Length == 0)
                return root;

            var currentNode = root;
            foreach (var pathPart in path)
            {
                var isFound = false;
                foreach (var child in currentNode.ChildNodes)
                {
                    if (child.Name == pathPart)
                    {
                        currentNode = child;
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                    return null;
            }

            return currentNode;
        }

        protected IEnumerable<Node> GetDescendants(Node root, params string[] path)
        {
            var node = GetDescendant(root, OmitLastPart(path));

            if (node == null)
                return new Node[0];

            var childName = path.Last();


            var result = new List<Node>();
            foreach (var child in node.ChildNodes)
            {
                if (child.Name == childName)
                    result.Add(child);
            }

            return result;
        }

        protected string[] GetTerminalTexts(Node root, params string[] path)
        {
            var node = GetDescendant(root, OmitLastPart(path));

            if (node == null)
                return new string[0];

            var result = new List<string>();
            foreach (var child in node.ChildNodes)
            {
                var text = GetTerminalText(child);
                if (text == null)
                    continue;

                result.Add(text);
            }

            return result.ToArray();
        }

        protected Node StepToChild(Node parent)
        {
            if (parent.ChildNodes.Count != 1)
                throw new NotSupportedException("Cannot step to child");

            return parent.ChildNodes[0];
        }

        /// <summary>
        /// Get text which was matched by given terminal node
        /// </summary>
        /// <param name="root">Terminal node which text will be returned</param>
        /// <returns>Terminal text if node is terminal, null otherwise</returns>
        protected string GetTerminalText(Node root, params string[] path)
        {
            var terminal = GetDescendant(root, path);

            if (terminal==null || terminal.Match == null)
                return null;

            return terminal.Match.MatchedData;
        }

        protected string GetSubTerminalText(Node terminalParent)
        {
            var terminal = StepToChild(terminalParent);
            if (terminal == null)
                return null;

            return GetTerminalText(terminal);
        }

        private string[] OmitLastPart(string[] pathParts)
        {
            return pathParts.Take(pathParts.Length - 1).ToArray();
        }

        #endregion

        #region Irony AST utilities

        protected string getName(ParseTreeNode node)
        {
            if (node == null)
                return null;

            return node.Term.Name;
        }

        protected ParseTreeNode getChild(string name, ParseTreeNode parent)
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
        protected ParseTreeNode stepToChild(ParseTreeNode parent)
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

        protected string getChildName(ParseTreeNode parent)
        {
            var child = stepToChild(parent);

            return getName(child);
        }

        protected IEnumerable<ParseTreeNode> getChilds(string selector, ParseTreeNode parent)
        {
            var pathParts = selector.Split('.');
            var targetName = pathParts.Last();
            var path = pathParts.Take(pathParts.Length - 1);

            //get node where children will be searched
            var node = GetNode(parent, path.ToArray());

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

        protected ParseTreeNode skipUnnamedChildren(ParseTreeNode node)
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

        protected ParseTreeNode findNode(ParseTreeNode root, string name)
        {
            var nodes = findNodes(root, name);
            if (nodes.Any())
                return nodes.First();

            return null;
        }

        protected IEnumerable<ParseTreeNode> findNodes(ParseTreeNode root, string name)
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

        protected ParseTreeNode GetNode(ParseTreeNode root, params string[] pathParts)
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

        protected bool hasName(ParseTreeNode node)
        {
            return !getName(node).StartsWith("Unnamed");
        }

        protected string[] findTerminals(string name, ParseTreeNode parent, string defaultValue = null)
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

        protected string GetTerminalText(ParseTreeNode terminal)
        {
            if (terminal.Token == null)
            {
                return null;
            }
            return terminal.Token.Text;
        }

        protected string getSubTerminal(ParseTreeNode node, string defaultValue = null, params string[] pathParts)
        {
            node = GetNode(node, pathParts);

            if (node.ChildNodes.Count != 1)
            {
                return defaultValue;
            }
            var termNode = node.ChildNodes[0];
            if (termNode.Token == null)
                //is not terminal
                return defaultValue;
            return GetTerminalText(termNode);
        }

        protected string[] getTerminals(string name, ParseTreeNode parent, string defaultValue = null)
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
