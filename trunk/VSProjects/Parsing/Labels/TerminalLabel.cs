using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    public class TerminalLabel:CompleteLabel
    {
        public readonly Terminal Terminal;

        public TerminalLabel(Terminal currentTerminal,GrammarSequence sequence)
            :base(sequence)
        {
            Terminal = currentTerminal;
        }
    }
}
