using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using Parsing.Source;

namespace Parsing
{
    public class PatternTerminal : Terminal
    {
        Regex _match;
        public PatternTerminal(string pattern, string name)
            : base(name)
        {
            _match = new Regex(@"\G" + pattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        }

        protected internal override TerminalMatch Match(SourceContext context)
        {
            context = context.SkipWhitespaces();
            var match = _match.Match(context.Text, context.Index);

            if (!match.Success)
                return new TerminalMatch(null, null, null);

            var shifted = context.Shift(match.Value.Length);

            return new TerminalMatch(shifted, context.Token, match.Value);
        }
    }
}
