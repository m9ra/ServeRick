using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    /// <summary>
    /// Represents single grammar rule, that can specify non terminal node behaviour
    /// </summary>
    public class GrammarRule
    {
        /// <summary>
        /// Grammar sequences contained in the rule
        /// </summary>
        private readonly List<GrammarSequence> _sequences = new List<GrammarSequence>();

        /// <summary>
        /// Grammar sequences contained in the rule
        /// </summary>
        internal IEnumerable<GrammarSequence> Sequences { get { return _sequences; } }

        /// <summary>
        /// Enumeration of terms in all contained sequences
        /// </summary>
        internal IEnumerable<Term> Terms
        {
            get
            {
                foreach (var sequence in Sequences)
                {
                    foreach (var term in sequence.Terms)
                    {
                        yield return term;
                    }
                }
            }
        }

        /// <summary>
        /// Enumeration of non terminals in all contained sequences
        /// </summary>
        internal IEnumerable<NonTerminal> NonTerminals
        {
            get
            {
                foreach (var term in Terms)
                {
                    if (term.Kind == TermKind.NonTerminal)
                        yield return term as NonTerminal;
                }
            }
        }

        /// <summary>
        /// Creates grammar rule from given sequences
        /// </summary>
        /// <param name="sequences">Sequences creating the rule</param>
        internal GrammarRule(params GrammarSequence[] sequences)
        {
            _sequences.AddRange(sequences);
        }

        /// <summary>
        /// Creates grammar rule from given sequences
        /// </summary>
        /// <param name="sequences">Sequences creating the rule</param>
        internal GrammarRule(IEnumerable<GrammarSequence> sequences) {
            _sequences.AddRange(sequences);
        }

        #region GrammarRule operator definitions

        /// <summary>
        /// Concat two grammar rules - rule for t1 has to appear on input before r2
        /// </summary>
        /// <param name="t1">Term for rule 1</param>
        /// <param name="r2">Rule 2</param>
        /// <returns>Concatenation of rules</returns>
        public static GrammarRule operator +(string t1, GrammarRule g2)
        {
            return Term.Unnamed(t1) + g2;
        }

        /// <summary>
        /// Concat two grammar rules - r1 has to appear on input before rule for t2
        /// </summary>
        /// <param name="r1">Rule 1</param>
        /// <param name="t2">Term for rule 2</param>
        /// <returns>Concatenation of rules</returns>
        public static GrammarRule operator +(GrammarRule g1, string t2)
        {
            return g1 + Term.Unnamed(t2);
        }
        
        /// <summary>
        /// Concat two grammar rules - r1 has to appear on input before r2
        /// </summary>
        /// <param name="r1">Rule 1</param>
        /// <param name="r2">Rule 2</param>
        /// <returns>Concatenation of rules</returns>
        public static GrammarRule operator +(GrammarRule r1, GrammarRule r2)
        {
            var sequences = new List<GrammarSequence>();

            foreach (var s1 in r1.Sequences)
            {
                foreach (var s2 in r2.Sequences)
                {
                    var terms = s1.Terms.Concat(s2.Terms);
                    sequences.Add(new GrammarSequence(terms));
                }
            }

            return new GrammarRule(sequences.ToArray());
        }

        /// <summary>
        /// Parallelize two rules - on input has to apear r1 or r2 rule
        /// </summary>
        /// <param name="r1">Rule 1</param>
        /// <param name="r2">Rule 2</param>
        /// <returns>Parallelization of rules</returns>
        public static GrammarRule operator |(GrammarRule r1, GrammarRule r2)
        {
            return new GrammarRule(r1.Sequences.Concat(r2.Sequences));
        }

        /// <summary>
        /// Convert given terminal word into grammar rule
        /// </summary>
        /// <param name="terminalWord">Word that has to appear on input</param>
        /// <returns>Created rule</returns>
        public static implicit operator GrammarRule(string terminalWord)
        {
            return Term.Unnamed(terminalWord);
        }

        /// <summary>
        /// Convert given sequence into grammar rule
        /// </summary>
        /// <param name="sequence">Sequence that has to appear on input</param>
        /// <returns>Created rule</returns>
        public static implicit operator GrammarRule(GrammarSequence sequence)
        {
            return new GrammarRule(sequence);
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (var seq in Sequences)
            {
                result.AppendLine(seq.ToString());
            }

            return result.ToString();
        }
        #endregion
    }
}
