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
        private Token _child;

        public readonly string Name;

        public readonly string Data;

        public bool IsSpecial { get { return Data == null; } }

        public readonly int StartPosition;

        public readonly int EndPosition;

        public int Length { get { return IsSpecial ? 0 : Data.Length; } }

        public Token Parent { get; private set; }

        public Token Child
        {
            get { return _child; }
            set
            {
                if (_child != null)
                    throw new NotSupportedException("Cannot set child twice");

                if (value == null)
                    throw new ArgumentNullException("child");

                _child = value;
                _child.Parent = this;
            }
        }

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
                EndPosition = StartPosition + Length;
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
