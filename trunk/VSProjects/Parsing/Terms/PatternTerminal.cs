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
        private readonly Regex _match;

        private readonly HashSet<string> _excludes = new HashSet<string>();

        public PatternTerminal(string pattern, string name)
            : base(name)
        {
            _match = new Regex(@"\G" + pattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        }

        protected internal override TerminalMatch Match(SourceContext context)
        {
            context = context.SkipWhitespaces();
            if (context==null)
                return TerminalMatch.Failed();

            var match = _match.Match(context.Text, context.Index);
            var value = match.Value;

            if (!match.Success || _excludes.Contains(value))
                return TerminalMatch.Failed();

            var shifted = context.Shift(value.Length);

            return new TerminalMatch(shifted, context.Token, value);
        }

        public PatternTerminal Exclude(string word)
        {
            _excludes.Add(word);
            return this;
        }
    }
}
