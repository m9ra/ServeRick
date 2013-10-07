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
        /// Token where matched data has been found
        /// </summary>
        public readonly Token MatchedToken;

        internal TerminalMatch(SourceContext matchEnd, Token matchedToken, string matchedData)
        {
            MatchEnd = matchEnd;
            Success = matchEnd != null;
            MatchedData = matchedData;
            MatchedToken = matchedToken;
        }

        public override string ToString()
        {
            var info = Success ? "'" + MatchedData + "'" : "Fail";
            return "[TerminalMatch]" + MatchedData;
        }
    }
}
