using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    /// <summary>
    /// Represents sequence of terms, which has to appear on input in order specified by current sequence
    /// </summary>
    public class GrammarSequence
    {
        /// <summary>
        /// Non terminal containing current sequence
        /// </summary>
        private  NonTerminal _parent = null;

        /// <summary>
        /// Terms contained in sequence in defined order
        /// </summary>
        private readonly List<Term> _terms = new List<Term>();

        /// <summary>
        /// Non terminal containing current sequence
        /// </summary>
        public NonTerminal Parent { get { return _parent; } }

        /// <summary>
        /// Terms contained in sequence in defined order
        /// </summary>
        public IEnumerable<Term> Terms { get { return _terms; } }

        public GrammarSequence(params Term[] terms)
        {
            _terms.AddRange(terms);
        }

        public GrammarSequence(IEnumerable<Term> terms) {
            _terms.AddRange(terms);
        }

        /// <summary>
        /// Set parent for current sequence
        /// </summary>
        /// <param name="parent">Non terminal containing current sequence</param>
        internal void SetParent(NonTerminal parent)
        {
            if (_parent != null)
                throw new NotSupportedException("Cannot use grammar sequence for two parents");

            _parent = parent;
        }

        public override string ToString()
        {
            var termStrings = new List<string>();

            foreach (var term in _terms)
            {
                termStrings.Add(term.ToString());
            }

            return "{" + string.Join(", ", termStrings.ToArray()) + "}";
        }
    }
}
