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
            _match = new Regex(string.Format(@"\G({0})", pattern), RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        }

        protected internal override TerminalMatch Match(SourceContext context)
        {
            var startContext = context.SkipWhitespaces();
            if (startContext == null)
                return TerminalMatch.Failed;

            var match = _match.Match(startContext.Text, startContext.Index);
            var value = match.Value;

            if (!match.Success || _excludes.Contains(value))
                return TerminalMatch.Failed;

            var currentContext = startContext;
            while (currentContext.Index < startContext.Index + value.Length - 1)
            {
                if (currentContext.Token.IsSpecial)
                {
                    //pattern terminal cannot cross special tokens
                    return TerminalMatch.Failed;
                }

                currentContext = currentContext.NextContext;
            }

            return new TerminalMatch(currentContext.NextContext, value);
        }

        public PatternTerminal Exclude(params string[] word)
        {
            _excludes.UnionWith(word);
            return this;
        }
    }
}
