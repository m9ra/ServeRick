using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    /// <summary>
    /// Token representing part of input. There can be special tokens 
    /// </summary>
    public class Token
    {
        public readonly string Name;

        public readonly string Data;

        public bool IsSpecial { get { return Data == null; } }

        public readonly int StartPosition;

        public readonly int EndPosition;

        public int Length { get { return IsSpecial ? 0 : Data.Length; } }

        public Token Child { get; internal set; }

        private Token(string tokenName, string data, int start)
        {
            Name = tokenName;
            Data = data;
            StartPosition = start;

            if (IsSpecial)
            {
                EndPosition = StartPosition;
            }
            else
            {
                //TODO this has pure semantic, refactor it
                var length = Length == 0 ? 0 : Length - 1; 
                EndPosition = StartPosition + length;
            }

            if (EndPosition < StartPosition)
                throw new NotSupportedException("End cannot be before start");
        }

        public static Token Special(string tokenName, int startPosition)
        {
            var token = new Token(tokenName, null, startPosition);
            return token;
        }

        public static Token Text(string data, int startPosition)
        {
            var token = new Token("Text", data, startPosition);
            return token;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]{2}", Name, StartPosition, Data);
        }
    }
}
