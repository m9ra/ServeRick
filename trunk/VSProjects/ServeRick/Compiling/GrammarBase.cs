using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony;
using Irony.Parsing;
using Irony.Ast;
using Irony.Parsing.Construction;

namespace ServeRick.Compiling
{
    public abstract class GrammarBase:Irony.Parsing.Grammar
    {
        protected NonTerminal NT(string name)
        {
            return new NonTerminal(name);
        }

        protected Terminal T(string value)
        {
            return ToTerm(value);
        }

        protected KeyTerm T_HIGH(string terminal)
        {
            var term = ToTerm(terminal);
            term.Priority = TerminalPriority.High;
            return term;
        }

        protected RegexBasedTerminal T_REG(string name, string pattern)
        {
            var result=new RegexBasedTerminal(name, pattern);
          //  result.Flags =TermFlags.
            return result;
        }

        protected IdentifierTerminal T_ID(string name)
        {
            return new IdentifierTerminal(name);
        }

        public override string ConstructParserErrorMessage(ParsingContext context, StringSet expectedTerms)
        {
            var expected = base.ConstructParserErrorMessage(context, expectedTerms);

            return string.Format("{0}\n but got: {1}", expected, context.CurrentParserInput);
        }
    }
}
