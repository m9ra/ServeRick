using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing.Source
{
    class SourceData
    {
        private readonly Dictionary<int, IncommingEdges> _incommingEdges = new Dictionary<int, IncommingEdges>();

        /// <summary>
        /// Contains contexts for every index in input. At every index there is first token on stream (multiple tokens on same index can start)
        /// </summary>
        private readonly SourceContext[] _contexts;

        private SourceContext _lastContext;

        internal readonly string Text;

        internal readonly Dictionary<int, Dictionary<Terminal, TerminalMatch>> Matches = new Dictionary<int, Dictionary<Terminal, TerminalMatch>>();

        internal SourceContext StartContext { get { return GetSourceContext(0); } }

        internal SourceData(string text, TokenStream sourceTokens)
        {
            Text = text;
            _contexts = new SourceContext[text.Length + 1];

            prepareContexts(sourceTokens);
        }

        private void registerContext(int index, Token token)
        {
            if (index > token.EndPosition || index < token.StartPosition)
                throw new NotSupportedException("Invalid token cover");

            _lastContext = new SourceContext(this, index, token, _lastContext);

            var context = _contexts[index];
            if (context == null || context.Token.StartPosition != index)
                //set context if there is no context or contained context is started elsewhere
                _contexts[index] = _lastContext;
        }

        private void prepareContexts(TokenStream sourceTokens)
        {
            var token = sourceTokens.NextToken();
            for (var index = 0; index < _contexts.Length; ++index)
            {
                registerContext(index, token);
                if (token.EndPosition <= index)
                {
                    token = sourceTokens.NextToken();
                    if (token != null && token.StartPosition == index)
                        --index;
                }
            }
        }

        /// <summary>
        /// Terminals that are requested by some incomming edges on given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal IEnumerable<TerminalLabel> WaitingLabels(int index)
        {
            var incomming = _incommingEdges[index];
            var terminals = incomming.WaitingTerminals;
            return terminals;
        }

        internal void ClearWaintingLabels(int index)
        {
            var incomming = _incommingEdges[index];
            incomming.ClearWaitingTerminals();
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
            if (index >= _contexts.Length)
                return _lastContext;

            return _contexts[index];
        }
    }
}
