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

            if(IsSpecial){
                EndPosition = StartPosition;
            }else{
                EndPosition = StartPosition + Length - 1;
            }
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


    }
}
