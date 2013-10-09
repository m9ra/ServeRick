using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    class IncommingEdges
    {
        private HashSet<ActiveEdge> _activeEdges = new HashSet<ActiveEdge>();
        private HashSet<CompleteEdge> _completeEdges = new HashSet<CompleteEdge>();
        private HashSet<TerminalLabel> _waitingTerminals = new HashSet<TerminalLabel>();
        
        private Dictionary<Term, HashSet<ActiveEdge>> _extensibleEdges = new Dictionary<Term, HashSet<ActiveEdge>>();

        public IEnumerable<TerminalLabel> WaitingTerminals { get { return _waitingTerminals; } }

        internal IEnumerable<ActiveEdge> ExtensibleWith(CompleteEdge edge)
        {         
            HashSet<ActiveEdge> extensible;
            var term = edge.Parent;
            if(_extensibleEdges.TryGetValue(term, out extensible))
                return extensible;
            return new ActiveEdge[0];
        }

        internal bool Connect(ActiveEdge edge)
        {
            if (!_activeEdges.Add(edge))
                return false;

            var currentTerm = edge.Label.CurrentTerm;
            var terminal = edge.Label.TerminalLabel;
            if (terminal != null)
                //label is waiting for terminal
                _waitingTerminals.Add(terminal);


            HashSet<ActiveEdge> extensible;
            if (!_extensibleEdges.TryGetValue(currentTerm, out extensible))
            {
                extensible = new HashSet<ActiveEdge>();
                _extensibleEdges[currentTerm] = extensible;
            }

            extensible.Add(edge);
            return true;
        }

        internal bool Connect(CompleteEdge edge)
        {
            if(!_completeEdges.Add(edge))
                return false;

            return true;
        }

        internal void ClearWaitingTerminals()
        {
            _waitingTerminals.Clear();
        }
    }
}
