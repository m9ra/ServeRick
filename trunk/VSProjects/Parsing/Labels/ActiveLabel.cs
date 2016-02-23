using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    public class ActiveLabel
    {
        private readonly int _index;
        private ActiveLabel[] _transitions;
        private readonly GrammarSequence _sequence;
        public readonly ActiveLabel NextInChain;
        public readonly Term CurrentTerm;
        public readonly bool WillComplete;
        public readonly CompleteLabel CompleteLabel;
        

        public readonly TerminalLabel TerminalLabel;

        public IEnumerable<ActiveLabel> Transitions { get { return _transitions; } }

        internal ActiveLabel(GrammarSequence sequence, int index = 0)
        {
            _sequence = sequence;
            _index = index;

            var sequenceTerms = sequence.Terms;
            CurrentTerm = sequenceTerms.Skip(index).First();
            WillComplete = sequenceTerms.Count() == index + 1;

            var currentTerminal = CurrentTerm as Terminal;
            if (currentTerminal != null)
                TerminalLabel = new TerminalLabel(currentTerminal, sequence);

            if (WillComplete)
            {
                CompleteLabel = new CompleteLabel(_sequence);
            }
            else
            {
                NextInChain = new ActiveLabel(sequence, index + 1);
            }
        }

        internal void SetTransitions(IEnumerable<ActiveLabel> transitions)
        {
            _transitions = transitions.ToArray();
        }

        public override string ToString()
        {
            return string.Format("{0}--{2}-->{1}",_sequence.Parent,_sequence,_index);
        }
    }
}
