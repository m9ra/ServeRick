using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing.Source
{
    public class SourceContext
    {
        private readonly SourceData _data;

        internal IncommingEdges IncommingEdges { get { return _data.Incomming(Index); } }

        public readonly int Index;

        public readonly Token Token;

        public string Text { get { return _data.Text; } }

        public string CurrentText { get { return _data.Text.Substring(Index); } }

        public bool EOF { get { return _data.Text.Length <= Index + 1; } }

        public bool BOF { get { return Index == 0; } }

        public readonly SourceContext PreviousContext;

        public SourceContext NextContext { get; private set; }

        internal SourceContext(SourceData data, int currentIndex, Token sourceToken, SourceContext previousContext)
        {

            if (sourceToken == null)
                throw new ArgumentNullException("sourceToken");

            _data = data;
            Index = currentIndex;
            PreviousContext = previousContext;
            Token = sourceToken;
            if (PreviousContext != null)
            {
                PreviousContext.NextContext = this;
            }
        }

        internal TerminalMatch Match(Terminal terminal)
        {
            Dictionary<Terminal, TerminalMatch> terminalMatchings;
            if (!_data.Matches.TryGetValue(Index, out terminalMatchings))
            {
                terminalMatchings = new Dictionary<Terminal, TerminalMatch>();
                _data.Matches[Index] = terminalMatchings;
            }

            TerminalMatch result;
            if (!terminalMatchings.TryGetValue(terminal, out result))
            {
                result = terminal.Match(this);
                terminalMatchings.Add(terminal, result);
            }

            return result;
        }

        internal TerminalMatch MatchSpecial(string specialTokenName)
        {
            var context = this.Token.IsSpecial ? this : this.SkipSpecialTokenWhitespaces();
            /*    while (context != null && context.Token.IsSpecial)
                {
                    var token = context.Token;
                    if (token.Name == specialTokenName)
                    {
                        if (context.NextContext != null)
                            context = context.NextContext;

                        return new TerminalMatch(context, token, null);
                    }

                    context = context.NextContext;
                }*/

            if (context.Token.IsSpecial && context.Token.Name == specialTokenName)
            {
                if (context.NextContext != null)
                    context = context.NextContext;

                return new TerminalMatch(context, context.Token, null);
            }

            return new TerminalMatch(null, null, null);
        }

        internal SourceContext Shift(string data)
        {
            if (Text.Length - Index < data.Length)
                return null;

            if (Text.Substring(Index, data.Length) == data)
            {
                return _data.GetSourceContext(Index + data.Length);
            }
            return null;
        }

        internal SourceContext Shift(int length)
        {
            return _data.GetSourceContext(Index + length);
        }

        internal SourceContext SkipWhitespaces()
        {
            if (this.Token.IsSpecial)
                //cant cross special token
                return null;

            var context = this;
            while (context.NextContext != null)
            {
                if (!char.IsWhiteSpace(Text, context.Index))
                    break;

                context = context.NextContext;
            }

            var nextContext = context;
            while (nextContext.Index == context.Index)
            {
                if (nextContext.Token.IsSpecial)
                    return null;

                nextContext = nextContext.NextContext;
            }

            return context;
        }

        internal SourceContext SkipSpecialTokenWhitespaces()
        {
            if (Token.IsSpecial)
                return this;

            var token = this.Token;
            while (!token.IsSpecial)
            {
                token = token.Child;
            }

            for (int i = Index; i < token.EndPosition; ++i)
            {
                if (!char.IsWhiteSpace(Text, i))
                    return this;
            }

            return _data.GetSourceContext(token.StartPosition);
        }


        internal IEnumerable<CompleteEdge> GetInterpretations()
        {
            var result = new List<CompleteEdge>();
            var labels = _data.WaitingLabels(Index).ToArray();
            foreach (var label in labels)
            {
                var terminalMatch = label.Terminal.Match(this);
                if (terminalMatch.Success)
                    result.Add(new TerminalEdge(label, this, terminalMatch));
            }

            return result;
        }

        internal void CleanWaitingLabels()
        {
            _data.ClearWaintingLabels(Index);
        }

        /// <summary>
        /// Connect Edges pointing from current to current context, created from given labels
        /// </summary>
        /// <param name="labels">Labels which edges will be created</param>
        internal void AddSelfEdgesFrom(IEnumerable<ActiveLabel> labels)
        {
            foreach (var label in labels)
            {
                var edge = new ActiveEdge(label, this, this);
                _data.Connect(edge);
            }
        }

        public override string ToString()
        {
            return "[Pos]" + Index;
        }
    }
}
