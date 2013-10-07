using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing.Source
{
    internal class TokenStream
    {
        private Token _lastToken;
        private readonly IEnumerator<Token> _tokensEnumerator;
        internal TokenStream(IEnumerable<Token> tokens)
        {
            _tokensEnumerator = tokens.GetEnumerator();
        }

        internal Token NextToken()
        {
            if (!_tokensEnumerator.MoveNext())
                //there is no next token
                return null;

            var currentToken = _tokensEnumerator.Current;

            if (_lastToken != null)
                //set child edge on tokens chain
                _lastToken.Child = currentToken;

            _lastToken = currentToken;
            return currentToken;
        }
    }
}
