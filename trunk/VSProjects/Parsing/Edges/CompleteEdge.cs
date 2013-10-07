using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing
{
    class CompleteEdge : Edge
    {
        internal readonly Term Parent;
        internal readonly CompleteLabel CompleteLabel;
        internal readonly ActiveEdge ExtendedEdge;
        internal readonly CompleteEdge ExtendingEdge;

        public bool IsFinal { get { return CompleteLabel.IsFinal; } }

        protected CompleteEdge(TerminalLabel completeLabel, SourceContext startContext, SourceContext endContext)
            : base(startContext, endContext)
        {
            Parent = completeLabel.Terminal;
            CompleteLabel = completeLabel;
        }


        public CompleteEdge(ActiveEdge extendedEdge, CompleteEdge extendingEdge)
            : base(extendedEdge.StartContext, extendingEdge.EndContext)
        {
            CompleteLabel = extendedEdge.Label.CompleteLabel;
            Parent = CompleteLabel.Parent;
            ExtendedEdge = extendedEdge;
            ExtendingEdge = extendingEdge;
        }

        public override string ToString()
        {
            return base.ToString() + CompleteLabel.ToString();
        }

        public override bool Equals(object obj)
        {
            var o = obj as CompleteEdge;
            if (o == null)
            {
                return false;
            }

            return o.CompleteLabel == CompleteLabel &&
                o.Parent == Parent &&
                o.EndContext== EndContext &&
            o.StartContext == StartContext;
        }

        public override int GetHashCode()
        {
            var result = EndContext.GetHashCode();
            if (CompleteLabel != null)
                result += CompleteLabel.GetHashCode();

            if (Parent != null)
                result += Parent.GetHashCode();

            return result;
        }
    }
}
