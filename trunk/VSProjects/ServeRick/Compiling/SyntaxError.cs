using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing;
using Parsing.Source;

namespace ServeRick.Compiling
{
    /// <summary>
    /// Representation of syntax error
    /// </summary>
    public class SyntaxError
    {
        /// <summary>
        /// Error location
        /// </summary>
        public readonly SourceContext Location;

        /// <summary>
        /// Error description
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Preview around error location
        /// </summary>        
        /// <returns>Preview representation</returns>
        public string NearPreview
        {
            get
            {
                //find start index
                var startIndex = Location.Index - 1;
                while (startIndex > 0)
                {
                    var ch = Location.Text[startIndex];
                    if (ch == '\n')
                        break;

                    --startIndex;
                }

                //find end index
                var endIndex = Location.Index + 1;
                while (endIndex < Location.Text.Length)
                {
                    var ch = Location.Text[endIndex];
                    if (ch == '\n')
                        break;

                    ++endIndex;
                }

                startIndex = Math.Max(0, startIndex);
                endIndex = Math.Min(Location.Text.Length, endIndex);

                return Location.Text.Substring(startIndex, endIndex - startIndex);

            }
        }

        /// <summary>
        /// String representation of line with given index.
        /// </summary>
        public string GetLine(int line)
        {
            var lines = Location.Text.Split('\n');
            if (line < 0 || line >= lines.Length)
                return "";

            return lines[line];
        }

        /// <summary>
        /// Tokens that comes from given line.
        /// </summary>
        public IEnumerable<Token> GetLineTokens(int line)
        {
            //find first token on the line
            var currentToken = Location.Token;

            while (currentToken.Child != null && startLine(currentToken) < line)
            {
                currentToken = currentToken.Child;
            }

            while (startLine(currentToken.Parent) >= line)
            {
                currentToken = currentToken.Parent;
            }

            //iterate until last token on the line
            var result = new List<Token>();
            while (startLine(currentToken) == line)
            {
                result.Add(currentToken);
                currentToken = currentToken.Child;
            }

            return result;
        }

        /// <summary>
        /// Initialize syntax error object
        /// </summary>
        /// <param name="location">Context where error is located</param>
        /// <param name="description">Description of error</param>
        public SyntaxError(SourceContext location, string description)
        {
            Description = description;
            Location = location;
        }

        private int startLine(Token token)
        {
            if (token == null)
                return -1;

            var lineIndex = 0;
            for (var i = 0; i < token.StartPosition; ++i)
            {
                if (Location.Text[i] == '\n')
                    lineIndex += 1;
            }

            return lineIndex;
        }

    }
}
