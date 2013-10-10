using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing.Source
{
    class SourceData
    {
        /// <summary>
        /// Contains contexts for every index in input. At every index there is first token on stream (multiple tokens on same index can start)
        /// </summary>
        private readonly SourceContext[] _contexts;

        private SourceContext _lastContext;

        private SourceContext _startContext;

        internal readonly string Text;

        internal readonly Dictionary<int, Dictionary<Terminal, TerminalMatch>> Matches = new Dictionary<int, Dictionary<Terminal, TerminalMatch>>();

        internal SourceContext StartContext { get { return _startContext; } }

        internal SourceData(string text, TokenStream sourceTokens)
        {
            Text = text;
            //length is increased for end position
            _contexts = new SourceContext[text.Length + 1];

            prepareContexts(sourceTokens);
        }

        private void registerContext(int index, Token token)
        {
            if (index > token.EndPosition || index < token.StartPosition)
                throw new NotSupportedException("Invalid token cover");

            _lastContext = new SourceContext(this, index, token, _lastContext);
            if (_startContext == null)
                _startContext = _lastContext;

            var context = _contexts[index];
            if (context == null || context.Token.StartPosition != index)
                //set context if there is no context or contained context is started elsewhere
                _contexts[index] = _lastContext;
        }

        private void prepareContexts(TokenStream sourceTokens)
        {
            Token token;
            while ((token = sourceTokens.NextToken()) != null)
            {
                if (token.StartPosition == token.EndPosition)
                {
                    registerContext(token.StartPosition, token);
                }
                else
                {
                    for (var i = token.StartPosition; i < token.EndPosition; ++i)
                    {
                        registerContext(i, token);
                    }
                }
            }

            if (sourceTokens.NextToken() != null)
                throw new NotSupportedException("Tokens are out of range");
        }
    }
}
