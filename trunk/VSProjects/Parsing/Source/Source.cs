using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsing
{
    public class Source
    {
        public readonly string Text;
        
        public Source(string data)
        {
            Text = data;
        }

        internal SourceContext CreateStartContext(SourceData data)
        {
            return new SourceContext(data, 0);
        }                
    }
}
