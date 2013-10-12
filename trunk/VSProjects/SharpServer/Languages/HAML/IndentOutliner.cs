using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing;

namespace SharpServer.Languages.HAML
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
        /// Number of spaces to cover single tabulator
        /// </summary>
        private readonly int _tabLength;

        /// <summary>
        /// Determine indentation level for each processed line (only non empty lines are considered)
        /// </summary>
        private readonly Dictionary<int, int> _indentationLevels = new Dictionary<int, int>();

        /// <summary>
        /// Input tokens that will be outlined
        /// </summary>
        private readonly IEnumerable<Token> _inputTokens;

        /// <summary>
        /// Line start of currently processed line
        /// </summary>
        private int _currentLineStart;

        /// <summary>
        /// Text start of currently processed linie
        /// </summary>
        private int _currentTextStart;

        /// <summary>
        /// Indentation level of currently processed line
        /// </summary>
        private int _currentIndentationLevel;

        /// <summary>
        /// Indentation level of last processed line
        /// </summary>
        private int _lastIndentation;


        /// <summary>
        /// Initialize outliner
        /// </summary>
        /// <param name="inputTokens">Tokens that will be outlined</param>
        /// <param name="tabLength">Number of spaces to cover single tabulator</param>
        internal IndentOutliner(IEnumerable<Token> inputTokens, int tabLength)
        {
            _tabLength = tabLength;
            _inputTokens = inputTokens;
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
                    //Comments has to be present inside single token
                    result.Add(token);
                }
                else
                {
                    outline(token, result);
                }
            }

            //add implicit dedentation before end of file  
            var lastIndex = result.Count - 1;
            var lastToken = result[lastIndex];
            result.RemoveAt(lastIndex);

            addTokens(lastToken.StartPosition, Dedent, _currentIndentationLevel, result);
            result.Add(lastToken);

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
            var isTextStarted = false;
            _currentTextStart = 0;
            _currentLineStart = 0;

            var lineIndentation = 0;
            for (int i = 0; i < token.Length; ++i)
            {
                var ch = token.Data[i];

                switch (ch)
                {
                    case '\n':
                        if (isTextStarted)
                        {
                            outlineText(token, i, lineIndentation, result);
                            isTextStarted = false;
                            _currentLineStart = i + 1;
                        }
                        lineIndentation = 0;
                        //else lineStart is left from previous line
                        //it signalizes empty line
                        break;

                    case ' ':
                        if (!isTextStarted)
                            lineIndentation += 1;
                        break;

                    case '\t':
                        if (!isTextStarted)
                            lineIndentation += _tabLength;
                        break;

                    default:
                        if (!isTextStarted)
                        {
                            //non whitespace char apeared - text is starting
                            isTextStarted = true;
                            _currentTextStart = i;
                        }
                        break;
                }
            }

            //process last line
            var endPosition = token.EndPosition;
            if (!isTextStarted)
                _currentTextStart = endPosition;

            outlineText(token, endPosition, lineIndentation, result);
        }

        private void outlineText(Token token, int lineEnd, int lineIndentation, List<Token> result)
        {
            var startOffset = token.StartPosition;

            addIndentation(startOffset, lineIndentation, result);

            if (_currentLineStart != _currentTextStart)
            {
                //add prefix token
                var prefix = token.Data.Substring(_currentLineStart, _currentTextStart - _currentLineStart);
                var prefixToken = Token.Text(prefix, _currentLineStart + startOffset);
                result.Add(prefixToken);
            }

            var text = token.Data.Substring(_currentTextStart, lineEnd - _currentTextStart);
            var textToken = Token.Text(text, _currentTextStart + startOffset);
            var eol = Token.Special(EOL, lineEnd + startOffset);

            result.Add(textToken);
            result.Add(eol);
        }

        private void addIndentation(int startOffset, int lineIndentation, List<Token> result)
        {
            if (_indentationLevels.Count > 0)
            {
                int indentationChange;
                string indentationTokenName;
                if (_lastIndentation < lineIndentation)
                {
                    //indentation increased
                    ++_currentIndentationLevel;
                    indentationChange = 1;
                    indentationTokenName = Indent;

                }
                else
                {
                    //indentation decreased or stay same
                    indentationChange = computeDedentLevel(lineIndentation,_currentIndentationLevel);
                    _currentIndentationLevel -= indentationChange;
                    indentationTokenName = Dedent;
                }

                addTokens(_currentLineStart + startOffset, indentationTokenName, indentationChange, result);
            }

            _lastIndentation = lineIndentation;
            _indentationLevels[lineIndentation] = _currentIndentationLevel;
        }

        private void addTokens(int tokenStart, string specialTokenName, int tokenCount, List<Token> result)
        {
            for (int i = 0; i < tokenCount; ++i)
            {
                var token = Token.Special(specialTokenName, tokenStart);
                result.Add(token);
            }
        }

        private int computeDedentLevel(int indentation, int currentLevel)
        {
            int level;
            if (_indentationLevels.TryGetValue(indentation, out level))
                return currentLevel - level;

            throw new NotSupportedException("Inconsistent indentation detected (TODO error reporting)");
        }

    }
}
