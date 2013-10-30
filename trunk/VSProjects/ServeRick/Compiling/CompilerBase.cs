using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing;
using Parsing.Source;

namespace ServeRick.Compiling
{
    class CompilerBase
    {
        /// <summary>
        /// Emitter where compiled code is emitted
        /// </summary>
        protected readonly Emitter E;

        /// <summary>
        /// Root node of parsed input
        /// </summary>
        protected readonly Node Root;

        /// <summary>
        /// Initialize compiler
        /// </summary>
        /// <param name="emitter">Emitter where compiled code is emitted</param>
        protected CompilerBase(Node root, Emitter emitter)
        {
            Root = root;
            E = emitter;
        }

        #region AST utilities

        /// <summary>
        /// Print parsed data representation
        /// </summary>
        /// <param name="data">Data to be printed</param>
        /// <returns>String representation of printed data</returns>
        static protected string Print(SourceData data)
        {
            if (data.Root == null)
            {
                return PrintError(data);
            }
            else
            {
                return Print(data.Root);
            }
        }

        /// <summary>
        /// Print errors from parsed data
        /// </summary>
        /// <param name="data">Data to be printed</param>
        /// <returns>String representation of printed error</returns>
        static protected string PrintError(SourceData data)
        {
            var context = data.StartContext;
            var waitingContexts = new List<SourceContext>();
            SourceContext lastFilledContext=null;
            while (context != null)
            {
                if (context.IncommingEdges.WaitingLabels.Any())
                {
                    waitingContexts.Add(context);
                }

                context = context.NextContext;
            }

            var errors = new List<SyntaxError>();
            if (waitingContexts.Count == 0)
            {
                throw new NotImplementedException("Error when there is no waiting source context");
            }
            else
            {
                foreach (var waitingContext in waitingContexts)
                {
                    var terminals = from label in waitingContext.IncommingEdges.WaitingLabels select label.Terminal;
                    var waitingTerms = string.Join("', '", (object[])terminals.ToArray());
                    var description = string.Format("expected: '{0}', but got: '{1}'", waitingTerms, waitingContext.Token);

                    var error = new SyntaxError(waitingContext, description);
                    errors.Add(error);
                }
            }

            return PrintErrors(errors);
        }

        /// <summary>
        /// Print errors from parsed data
        /// </summary>
        /// <param name="errors">Errors to be printed</param>
        /// <returns>String representation of errors</returns>
        static protected string PrintErrors(IEnumerable<SyntaxError> errors)
        {
            var output = new StringBuilder();
            foreach (var error in errors)
            {
                output.AppendFormat("{0} at '{1}',\nNear: '{2}'\n\n",error.Description,error.Location,error.NearPreview);
            }

            return output.ToString();
        }

        /// <summary>
        /// Print node representation to console
        /// </summary>
        /// <param name="node">Root node of printed tree</param>
        /// <param name="level">Level of indentation</param>
        static protected string Print(Node node, int level = 0)
        {
            var output = new StringBuilder();
            output.AppendLine("".PadLeft(level * 2, ' ') + node);
            foreach (var child in node.ChildNodes)
            {
                output.Append(Print(child, level + 1));
            }

            return output.ToString();
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

        /// <summary>
        /// Get descendants at path relative to root
        /// </summary>
        /// <param name="root">Root node for path</param>
        /// <param name="path">Path for root children traversing</param>
        /// <returns>Found nodes</returns>
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

        /// <summary>
        /// Get texts from terminals at given path relative to root
        /// </summary>
        /// <param name="root">Root node for path</param>
        /// <param name="path">Path for root children traversing</param>
        /// <returns>Found terminal texts</returns>
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

        /// <summary>
        /// Get text which was matched by given terminal node
        /// </summary>
        /// <param name="root">Terminal node which text will be returned</param>
        /// <returns>Terminal text if node is terminal, null otherwise</returns>
        protected string GetTerminalText(Node root, params string[] path)
        {
            var terminal = GetDescendant(root, path);

            if (terminal == null || terminal.Match == null)
                return null;

            return terminal.Match.MatchedData;
        }

        /// <summary>
        /// Get terminal text from child of terminalParent
        /// </summary>
        /// <param name="terminalParent">Parent of terminal</param>
        /// <returns>Text matched by terminal</returns>
        protected string GetSubTerminalText(Node terminalParent)
        {
            var terminal = StepToChild(terminalParent);
            if (terminal == null)
                return null;

            return GetTerminalText(terminal);
        }

        /// <summary>
        /// Step to single child of parent
        /// </summary>
        /// <param name="parent">Parent of returned children</param>
        /// <returns>Child of given parent</returns>
        protected Node StepToChild(Node parent)
        {
            if (parent.ChildNodes.Count != 1)
                throw new NotSupportedException("Cannot step to child");

            return parent.ChildNodes[0];
        }

        /// <summary>
        /// Omit last part in path
        /// </summary>
        /// <param name="pathParts">Path parts</param>
        /// <returns>Path without last part</returns>
        private string[] OmitLastPart(string[] pathParts)
        {
            return pathParts.Take(pathParts.Length - 1).ToArray();
        }

        #endregion
    }
}
