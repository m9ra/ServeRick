using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    class KeyTerminal:Terminal
    {
        public readonly string Key;

        public KeyTerminal(string key,string name)
            : base(name)
        {
            Key = key;
        }

        protected internal override TerminalMatch Match(SourceContext context)
        {
            context=context.SkipWhitespaces();
            var shifted = context.Shift(Key);

            return new TerminalMatch(shifted,Key);
        }

        public override string ToString()
        {
            return Key;        
        }
    }
}
