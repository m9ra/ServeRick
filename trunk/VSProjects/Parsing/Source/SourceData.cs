using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    class SourceData
    {
        internal readonly Source Source;

        internal readonly Dictionary<int, Dictionary<Terminal, TerminalMatch>> Matches = new Dictionary<int, Dictionary<Terminal, TerminalMatch>>();

        private readonly Dictionary<int, IncommingEdges> _incommingEdges = new Dictionary<int, IncommingEdges>();

        private readonly Dictionary<int, SourceContext> _contexts = new Dictionary<int, SourceContext>();

        internal SourceData(Source source, GrammarBase grammar)
        {
            Source = source;
        }

        /// <summary>
        /// Terminals that are requested by some incomming edges on given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal IEnumerable<TerminalLabel> WaitingLabels(int index)
        {
            var incomming=_incommingEdges[index];

            return incomming.WaitingTerminals;
        }
        
        internal bool Connect(CompleteEdge edge)
        {
            //var outcomming = edge.StartContext.OutgoingEdges;
            var incomming = edge.EndContext.IncommingEdges;

            return incomming.Connect(edge);
            //outcomming.Connect(edge);
        }

        internal bool Connect(ActiveEdge edge)
        {
            //var outcomming= edge.StartContext.OutgoingEdges;
            var incomming = edge.EndContext.IncommingEdges;

            return incomming.Connect(edge);
            //outcomming.Connect(edge);
        }

        internal IncommingEdges Incomming(int index)
        {
            IncommingEdges edges;
            if (!_incommingEdges.TryGetValue(index, out edges))
            {
                edges = new IncommingEdges();
                _incommingEdges[index] = edges;
            }
            return edges;
        }

        internal SourceContext GetSourceContext(int index)
        {
            SourceContext context;
            if (!_contexts.TryGetValue(index, out context))
            {
                context = new SourceContext(this, index);
                _contexts[index] = context;
            }

            return context;
        }
    }
}
