using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    /// <summary>
    /// Kind of term
    /// </summary>
    public enum TermKind
    {
        /// <summary>
        /// Non terminal term of grammar
        /// </summary>
        NonTerminal,

        /// <summary>
        /// Terminal term of grammar
        /// </summary>
        Terminal
    }

    /// <summary>
    /// Term of grammar represents atomic part of rule
    /// </summary>
    public abstract class Term
    {
        /// <summary>
        /// Name of term
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Kind of term
        /// </summary>
        public readonly TermKind Kind;

        internal Term(string name, TermKind kind)
        {
            Name = name;
            Kind = kind;
        }

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator GrammarRule(Term t)
        {
            return new GrammarRule(new GrammarSequence(t));
        }

        public static GrammarRule operator +(Term t1, Term t2)
        {
            return new GrammarRule(new GrammarSequence(t1, t2));
        }

        public static GrammarRule operator |(Term t1, Term t2)
        {
            return new GrammarRule(new GrammarSequence(t1), new GrammarSequence(t2));
        }

        public static GrammarRule operator +(string terminal, Term t2)
        {
            return Unnamed(terminal) + t2;
        }

        public static GrammarRule operator +(Term t1, string terminal)
        {
            return t1 + Unnamed(terminal);
        }

        public static GrammarRule operator |(string terminal, Term t2)
        {
            return Unnamed(terminal) | t2;
        }

        public static GrammarRule operator |(Term t1, string terminal)
        {
            return t1 | Unnamed(terminal);
        }

        public static Terminal Unnamed(string keyword)
        {
            return new KeyTerminal(keyword, null);
        }
    }
}
