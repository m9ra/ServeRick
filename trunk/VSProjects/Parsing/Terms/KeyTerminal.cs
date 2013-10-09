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
            Key = key;
        }

        protected internal override TerminalMatch Match(SourceContext context)
        {
            context = context.SkipWhitespaces();
            if (context == null)
                return TerminalMatch.Failed();

            var shifted = context.Shift(Key);

            return new TerminalMatch(shifted, context.Token, Key);
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
