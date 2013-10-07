using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing
{
    /// <summary>
    /// Represents 
    /// </summary>
    class ActiveEdge : Edge
    {
        internal readonly ActiveLabel Label;

        internal readonly ActiveEdge ExtendedEdge;

        internal readonly CompleteEdge ExtendingEdge;



        internal ActiveEdge(ActiveLabel label, SourceContext startContext, SourceContext endContext)
            : base(startContext, endContext)
        {
            Label = label;
        }

        /// <summary>
        /// Creates self edge
        /// </summary>
        /// <param name="label"></param>
        /// <param name="extendedEdge"></param>
        internal ActiveEdge(ActiveLabel label, ActiveEdge extendedEdge)
            : base(extendedEdge.EndContext, extendedEdge.EndContext)
        {
            Label = label;
            //       ExtendedEdge = extendedEdge;
        }

        internal ActiveEdge(ActiveEdge extendedEdge, CompleteEdge extendingEdge)
            : base(extendedEdge.StartContext, extendingEdge.EndContext)
        {
            ExtendingEdge = extendingEdge;

            ExtendedEdge = extendedEdge;
            Label = ExtendedEdge.Label.NextInChain;
        }

        internal CompleteEdge CompleteBy(CompleteEdge constituent)
        {
            //  return new CompleteEdge(Label.CompleteLabel, OriginContext, constituent.EndContext);
            return new CompleteEdge(this, constituent);
        }

        internal ActiveEdge ExtendBy(CompleteEdge constituent)
        {
            //            return new ActiveEdge(Label.NextInChain, OriginContext, StartContext, constituent.EndContext);
            return new ActiveEdge(this, constituent);
        }

        public override string ToString()
        {
            return base.ToString() + Label.ToString();
        }

        public override bool Equals(object obj)
        {
            var o = obj as ActiveEdge;
            if (o == null)
            {
                return false;
            }

            return o.Label == Label &&
                   o.EndContext== EndContext &&
            o.StartContext == StartContext;
        }

        public override int GetHashCode()
        {
            return Label.GetHashCode() + EndContext.GetHashCode();
        }
    }
}
