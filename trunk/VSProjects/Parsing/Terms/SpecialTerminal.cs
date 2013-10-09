using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing.Terms
{
    class SpecialTerminal : Terminal
    {
        internal SpecialTerminal(string specialTokenName)
            : base(specialTokenName)
        {
        }

        protected internal override TerminalMatch Match(SourceContext context)
        {
            var currentContext = context.SkipSpecialTokenWhitespaces();
            return currentContext.MatchSpecial(Name);
        }
    }
}
