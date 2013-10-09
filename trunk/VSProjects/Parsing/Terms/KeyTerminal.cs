using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing
{
    class KeyTerminal : Terminal
    {
        public readonly string Key;

        public KeyTerminal(string key, string name)
            : base(name)
        {
            if (key.Length < 1)
                throw new NotSupportedException("Cannot match empty key, use empty  statemet instead");
            Key = key;
        }

        protected internal override TerminalMatch Match(SourceContext context)
        {
            var currentContext = context.SkipWhitespaces();
            
            for (var i = 0; i < Key.Length; ++i)
            {
                if (currentContext == null || currentContext.Token.IsSpecial)
                {
                    return TerminalMatch.Failed;
                }

                var currentChar = currentContext.IndexedChar;
                if (currentChar != Key[i])
                {
                    return TerminalMatch.Failed;
                }

                currentContext = currentContext.NextContext;
            }

            return new TerminalMatch(currentContext, Key);
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
