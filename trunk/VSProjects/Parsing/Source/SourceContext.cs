using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    public class SourceContext
    {
        private readonly SourceData _data;

        public readonly int Index;

        public Source Source { get { return _data.Source; } }

        public string Text { get { return Source.Text; } }

        public string CurrentText { get { return Source.Text.Substring(Index); } }

        public bool EOF { get { return Source.Text.Length <= Index; } }
        public bool BOF { get { return Index == 0; } }

        internal IncommingEdges IncommingEdges { get { return _data.Incomming(Index); } }

        internal SourceContext(SourceData data, int currentIndex)
        {
            _data = data;
            Index = currentIndex;
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

        internal SourceContext Shift(string data)
        {
            if (Source.Text.Length - Index < data.Length)
                return null;

            if (Source.Text.Substring(Index, data.Length) == data)
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
            for (int i = Index; i < Source.Text.Length; ++i)
            {
                if (!char.IsWhiteSpace(Source.Text,i))
                    return _data.GetSourceContext(i);
            }

            return _data.GetSourceContext(Text.Length);
        }
            

        internal IEnumerable<CompleteEdge> GetInterpretations()
        {
            var result = new List<CompleteEdge>();
            foreach (var label in _data.WaitingLabels(Index))
            {
                var terminalMatch = label.Terminal.Match(this);
                if (terminalMatch.Success)
                    result.Add(new TerminalEdge(label, this, terminalMatch));
            }

            return result;
        }

        /// <summary>
        /// Connect Edges pointing from current to current context, created from given labels
        /// </summary>
        /// <param name="labels">Labels which edges will be created</param>
        internal void AddSelfEdgesFrom(IEnumerable<ActiveLabel> labels)
        {
            foreach (var label in labels)
            {
                var edge = new ActiveEdge(label,this, this);
                _data.Connect(edge);
            }
        }

        public override string ToString()
        {
            return "[Pos]"+Index;
        }
    }
}
