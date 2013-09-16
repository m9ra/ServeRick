using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer
{
    /// <summary>
    /// Prototype for GET request specification
    /// </summary>
    public class GETAttribute:Attribute
    {
        public GETAttribute(string pattern)
        {
        }
    }
}
