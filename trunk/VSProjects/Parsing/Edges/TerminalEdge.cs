using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing
{
    class TerminalEdge:CompleteEdge
    {
        internal readonly TerminalMatch Match;
        internal TerminalEdge(TerminalLabel terminal, SourceContext startContext, TerminalMatch match)
            : base(terminal, startContext, match.MatchEnd)
        {
            Match = match;
        }

        public override string ToString()
        {
            return string.Format("({0},{1})",StartContext,EndContext)+ Match.ToString();
        }
    }
}
