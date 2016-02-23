using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing;

namespace ServeRick.Languages.HAML
{
    /// <summary>
    /// Provides outlining services for HAML. Can
    /// handle different styles of indentation. 
    /// </summary>
    class IndentOutliner
    {
        /// <summary>
        /// Marker for Indentation tokens
        /// </summary>
        public static readonly string Indent = "INDENT";

        /// <summary>
        /// Marker for Dedentation tokens
        /// </summary>
        public static readonly string Dedent = "DEDENT";

        /// <summary>
        /// Marker for End of line tokens
        /// </summary>
        public static readonly string EOL = "EOL";

        /// <summary>
        /// Marker for Begining of line tokens
        /// </summary>
        public static readonly string BOL = "BOL";

        /// <summary>
        /// Number of spaces to cover single tabulator
        /// </summary>
        private readonly int _tabLength;

        /// <summary>
        /// Determine indentation levels as they are nested.
        /// </summary>
        private readonly Stack<int> _indentationLevels = new Stack<int>();

        /// <summary>
        /// Input tokens that will be outlined
        /// </summary>
        private readonly Token[] _inputTokens;

        /// <summary>
        /// Initialize outliner
        /// </summary>
        /// <param name="inputTokens">Tokens that will be outlined</param>
        /// <param name="tabLength">Number of spaces to cover single tabulator</param>
        internal IndentOutliner(IEnumerable<Token> inputTokens, int tabLength)
        {
            _tabLength = tabLength;
            _inputTokens = inputTokens.ToArray();
        }

        /// <summary>
        /// Process oulining on input tokens. Result of oulining is returned.
        /// </summary>
        /// <returns>Outlined input.</returns>
        internal IEnumerable<Token> Outline()
        {
            var result = new List<Token>();
            foreach (var token in _inputTokens)
            {
                if (token.IsSpecial)
                {
                    //we leave special tokens as they are
                    result.Add(token);
                }
                else
                {
                    outline(token, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Process outlining on given text token. Tokens are stored in 
        /// result.
        /// </summary>
        /// <param name="token">Token to be outlined</param>
        /// <param name="result">Here are stored resulting tokens</param>
        private void outline(Token token, List<Token> result)
        {
            var lines = token.Data.Split('\n');
            var currentTokenOffset = 0;
            for (var i = 0; i < lines.Length; ++i)
            {
                var line = lines[i];
                var lineStartAbsoluteOffset = currentTokenOffset + token.StartPosition;

                //handle indentation
                var currentIndentationLength = getIndentationLength(line);
                var indentationLevelDelta = getIndentationLevelDelta(currentIndentationLength);
                appendIndentation(lineStartAbsoluteOffset, indentationLevelDelta, result);

                //add text token
                var lineText = line.TrimStart(' ', '\t');
                if (lineText.Length > 0)
                {
                    //we can skip empty lines
                    result.Add(Token.Special(BOL, lineStartAbsoluteOffset));
                    result.Add(Token.Text(lineText, lineStartAbsoluteOffset + currentIndentationLength));
                    result.Add(Token.Special(EOL, lineStartAbsoluteOffset + line.Length));
                }

                currentTokenOffset += line.Length + 1;
            }

            //add implicit indentation at the end
            appendIndentation(currentTokenOffset + token.StartPosition - 1, -getActualIndentationLevel(), result);
        }

        private int getActualIndentationLevel()
        {
            return _indentationLevels.Count;
        }

        private int getActualIndentationLength()
        {
            return _indentationLevels.Count > 0 ? _indentationLevels.Peek() : 0;
        }

        private int getIndentationLevelDelta(int newIndentationLength)
        {
            var currentIndentationLength = getActualIndentationLength();
            if (currentIndentationLength == newIndentationLength)
            {
                //nothing has changed
                return 0;
            }

            if (newIndentationLength > currentIndentationLength)
            {
                //indentation has been increased
                _indentationLevels.Push(newIndentationLength);
                return 1;
            }

            //indentation larger than the new one
            var decreaseLevel = -1;
            while (currentIndentationLength > newIndentationLength)
            {
                ++decreaseLevel;
                if (_indentationLevels.Count == 0)
                {
                    currentIndentationLength = 0;
                    break;
                }

                currentIndentationLength = _indentationLevels.Pop();
            }


            if (currentIndentationLength != newIndentationLength)
                throw new NotSupportedException("Inconsistent indentation detected");

            //renew the length
            if (newIndentationLength > 0)
                _indentationLevels.Push(currentIndentationLength);

            return -decreaseLevel;
        }

        private int getIndentationLength(string line)
        {
            var indentationLength = 0;
            for (var i = 0; i < line.Length; ++i)
            {
                var ch = line[i];
                if (ch == ' ')
                    indentationLength += 1;
                else if (ch == '\t')
                    indentationLength += _tabLength;
                else
                    //end of indentation
                    return indentationLength;
            }

            //empty line copy indentation length from preceding lines
            return getActualIndentationLength();
        }

        private void appendIndentation(int indentationAbsoluteOffset, int indentationDelta, List<Token> result)
        {
            var isIndent = indentationDelta > 0;
            var changeSize = Math.Abs(indentationDelta);
            for (var i = 0; i < changeSize; ++i)
            {
                var tokenType = isIndent ? Indent : Dedent;
                result.Add(Token.Special(tokenType, indentationAbsoluteOffset));

                //every indent/dedent except the last one will have preceding EOL
                var isLast = i + 1 == changeSize;
                /* if (!isLast)
                     result.Add(Token.Special(EOL, indentationAbsoluteOffset));*/
            }
        }
    }
}
