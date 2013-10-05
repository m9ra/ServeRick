using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    public class NonTerminal:Term
    {
        public bool IsFinal { get; internal set; }

        public GrammarRule Rule;        

        public NonTerminal(string name)
            :base(name,TermKind.NonTerminal)
        {
        }

        public override string ToString()
        {
            if (Name == null)
                return "[Unnamed]";

            return Name;
        }
    }
}
