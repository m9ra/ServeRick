using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    class TerminalLabel:CompleteLabel
    {
        internal readonly Terminal Terminal;

        public TerminalLabel(Terminal currentTerminal,GrammarSequence sequence)
            :base(sequence)
        {
            Terminal = currentTerminal;
        }
    }
}
