using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    /// <summary>
    /// Node representing parse tree
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Grammar, which rules created this node
        /// </summary>
        internal readonly GrammarBase Grammar;

        /// <summary>
        /// Edge from earley algorithm, which represent current node
        /// </summary>
        internal readonly CompleteEdge Edge;

        /// <summary>
        /// Terminal edge, available if node represents terminal
        /// </summary>
        internal TerminalEdge TerminalEdge { get { return Edge as TerminalEdge; } }

        /// <summary>
        /// Name of node, determined by grammar rules
        /// </summary>
        public readonly string Name;
      
        /// <summary>
        /// Terminal match of current node or null for non terminal nodes        
        /// </summary>
        public TerminalMatch Match { get { return TerminalEdge == null ? null : TerminalEdge.Match; } }

        /// <summary>
        /// Determine that node has name
        /// </summary>
        public bool HasName { get { return Name != null; } }

        /// <summary>
        /// Determine that node is transient
        /// </summary>
        public bool IsTransient { get { return !HasName || Grammar.IsTransient(Edge.Parent); } }

        /// <summary>
        /// List of child nodes of current node
        /// </summary>
        public readonly List<Node> ChildNodes = new List<Node>();

        /// <summary>
        /// Creates node in context of given grammar, from completeEdge
        /// </summary>
        /// <param name="grammar">Grammar which rules created current node</param>
        /// <param name="completeEdge">Edge that will be represented by created node</param>
        internal Node(GrammarBase grammar, CompleteEdge completeEdge)
        {
            Edge = completeEdge;
            Grammar = grammar;

            if (TerminalEdge == null)
            {
                Name = completeEdge.CompleteLabel.Parent.Name;
            }
            else
            {
                Name = TerminalEdge.Parent.Name;
            }

            addChild(completeEdge.ExtendingEdge);

            var current = completeEdge.ExtendedEdge;
            while (current != null)
            {
                addChild(current.ExtendingEdge);
                current = current.ExtendedEdge;
            }
            ChildNodes.Reverse();

            skipTransientChildren();
            removePunctuation();
        }

        /// <summary>
        /// Remove puncation from child nodes
        /// </summary>
        private void removePunctuation()
        {
            for (int i = 0; i < ChildNodes.Count; ++i)
            {
                var child = ChildNodes[i];

                if (child.TerminalEdge == null)
                    //only terminals can be puncation
                    continue;

                if (!Grammar.IsPunctuation(child.TerminalEdge.Match.MatchedData))
                    continue;


                ChildNodes.RemoveAt(i);
                i--;
            }
        }

        /// <summary>
        /// Skip transient nodes in children
        /// </summary>
        private void skipTransientChildren()
        {
            for (int i = 0; i < ChildNodes.Count; ++i)
            {
                var child = ChildNodes[i];

                if (!child.IsTransient|| child.ChildNodes.Count==0)
                    continue;

                ChildNodes.RemoveAt(i);
                ChildNodes.InsertRange(i, child.ChildNodes);

                i += child.ChildNodes.Count-1;
            }
        }

        /// <summary>
        /// Add child node representing given edge
        /// </summary>
        /// <param name="edge">Edge that will be represented by added child</param>
        private void addChild(CompleteEdge edge)
        {
            if (edge == null)
                return;

            var child = new Node(Grammar,edge);
            ChildNodes.Add(child);
        }

        public override string ToString()
        {
            if (Name == null)
                return string.Format("{0}", Match);

            if (Match == null)
                return Name;


            return string.Format("[{0}] {1}", Name, Match.MatchedData);
        }
    }
}
