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
            _contexts = new SourceContext[text.Length];

            prepareContexts(sourceTokens);
        }

        private void registerContext(int index, Token token)
        {
            _lastContext = new SourceContext(this, index, token, _lastContext);

            var context = _contexts[index];
            if (context == null || context.Token.StartPosition != index)
                //set context if there is no context or contained context is started elsewhere
                _contexts[index] = _lastContext;
        }

        private void prepareContexts(TokenStream sourceTokens)
        {
            var lastTokenEnd = 0;
            var token = sourceTokens.NextToken();
            //create contexts with token chains
            while (token != null)
            {
                var isRegistered = false;
                while (token!=null && lastTokenEnd >= token.EndPosition)
                {
                    token = sourceTokens.NextToken();
                    registerContext(lastTokenEnd, token);
                    isRegistered = true;
                }

                if (!isRegistered)
                {
                    registerContext(lastTokenEnd, token);
                }

                ++lastTokenEnd;
            }

            if (lastTokenEnd != _contexts.Length)
                throw new NotSupportedException("Input text is not covered by tokens");
        }

        /// <summary>
        /// Terminals that are requested by some incomming edges on given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal IEnumerable<TerminalLabel> WaitingLabels(int index)
        {
            var incomming = _incommingEdges[index];

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
            if (index >= _contexts.Length)
                //TODO repair context, to not need this
                index = _contexts.Length - 1;

            return _contexts[index];
        }
    }
}
