using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing.Source;

namespace Parsing
{
    public class Edge
    {
        internal readonly SourceContext StartContext;

        internal readonly SourceContext EndContext;

        protected Edge(SourceContext startContext, SourceContext endContext)
        {
            StartContext = startContext;
            EndContext = endContext;
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", StartContext, EndContext);
        }
    }
}
