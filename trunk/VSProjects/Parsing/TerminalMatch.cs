using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing
{
    /// <summary>
    /// Represents match of source input from terminal
    /// </summary>
    public class TerminalMatch
    {
        /// <summary>
        /// Determine that match has been successful
        /// </summary>
        public readonly bool Success;

        /// <summary>
        /// End of match in source
        /// </summary>
        public readonly SourceContext MatchEnd;

        /// <summary>
        /// Data that has been matched
        /// </summary>
        public readonly string MatchedData;

        /// <summary>
        /// Match used for unsucessfull matchings
        /// </summary>
        public static readonly TerminalMatch Failed = new TerminalMatch();

        private TerminalMatch()
        {
            Success = false;
        }

        internal TerminalMatch(SourceContext matchEnd, string matchedData)
        {
            if (matchEnd == null)
                throw new ArgumentNullException("matchEnd");

            Success = true;
            MatchEnd = matchEnd;
            MatchedData = matchedData;
        }

        public override string ToString()
        {
            var info = Success ? "'" + MatchedData + "'" : "Fail";
            return "[TerminalMatch]" + MatchedData;
        }
    }
}
