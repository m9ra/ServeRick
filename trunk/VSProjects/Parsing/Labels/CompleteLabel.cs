using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    public class CompleteLabel
    {
        /// <summary>
        /// Nonterminal which created this label from its grammar sequence                
        /// </summary>
        public readonly NonTerminal Parent;

        public readonly GrammarSequence Sequence;

        public bool IsFinal { get { return Parent.IsFinal; } }

        internal CompleteLabel(GrammarSequence sequence)
        {
            Sequence = sequence;
            Parent = sequence.Parent;
        }

        public override string ToString()
        {
            return string.Format("{0}->{1}",Parent, Sequence.ToString());
        }
    }
}
