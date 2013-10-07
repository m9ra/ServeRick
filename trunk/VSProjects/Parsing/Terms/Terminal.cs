using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing
{
    public abstract class Terminal : Term
    {
        public Terminal(string name)
            : base(name,TermKind.Terminal)
        {}

        internal protected abstract TerminalMatch Match(SourceContext context);      

        
    }
}
