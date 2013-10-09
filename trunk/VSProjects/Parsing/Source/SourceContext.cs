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

        internal readonly IncommingEdges IncommingEdges;

        public readonly int Index;

        public readonly Token Token;

        public string Text { get { return _data.Text; } }

        public string CurrentText { get { return _data.Text.Substring(Index); } }

        public bool EOF { get { return NextContext == null; } }

        public bool BOF { get { return Index == 0; } }

        public char IndexedChar
        {
            get
            {
                return Text[Index];
            }
        }

        public readonly SourceContext PreviousContext;

        public SourceContext NextContext { get; private set; }

        internal SourceContext(SourceData data, int currentIndex, Token sourceToken, SourceContext previousContext)
        {
            if (sourceToken == null)
                throw new ArgumentNullException("sourceToken");

            _data = data;
            IncommingEdges = new IncommingEdges();
            Index = currentIndex;
            PreviousContext = previousContext;
            Token = sourceToken;
            if (PreviousContext != null)
            {
                PreviousContext.NextContext = this;
            }
        }

        internal bool Connect(CompleteEdge edge)
        {
            var incomming = edge.EndContext.IncommingEdges;

            return incomming.Connect(edge);
        }

        internal bool Connect(ActiveEdge edge)
        {
            var incomming = edge.EndContext.IncommingEdges;

            return incomming.Connect(edge);
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
            var context = this;
            if (context.Token.IsSpecial && context.Token.Name == specialTokenName)
            {
                if (context.NextContext != null)
                    context = context.NextContext;

                return new TerminalMatch(context, null);
            }

            return TerminalMatch.Failed;
        }

        internal SourceContext SkipWhitespaces()
        {
            var context = this;

            while (context.NextContext != null)
            {
                if (context.Token.IsSpecial)
                    //cannot cross special tokens
                    return null;

                var nextChar = context.IndexedChar;
                if (!char.IsWhiteSpace(nextChar))
                {
                    //next char is not a whitespace
                    break;
                }

                context = context.NextContext;
            }

            return context;
        }

        internal SourceContext SkipSpecialTokenWhitespaces()
        {
            var context = this;

            while (context.NextContext != null)
            {
                if (context.Token.IsSpecial)
                    //skip only non-special tokens
                    break;

                var nextChar = context.IndexedChar;
                if (!char.IsWhiteSpace(nextChar))
                {
                    //next char is not a whitespace
                    break;
                }

                context = context.NextContext;
            }

            return context;
        }


        internal IEnumerable<CompleteEdge> GetInterpretations()
        {
            var result = new List<CompleteEdge>();
            var labels = IncommingEdges.WaitingTerminals.ToArray();
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
            IncommingEdges.ClearWaitingTerminals();
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
                IncommingEdges.Connect(edge);
            }
        }

        public override string ToString()
        {
            return "[Pos]" + Index;
        }
    }
}
