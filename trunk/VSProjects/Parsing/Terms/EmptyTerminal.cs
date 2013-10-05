using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    public class EmptyTerminal:Terminal
    {
        public EmptyTerminal()
            : base("EmptyTerminal")
        {
        }

        protected internal override TerminalMatch Match(SourceContext context)
        {
            return new TerminalMatch(context, "");
        }
    }
}
