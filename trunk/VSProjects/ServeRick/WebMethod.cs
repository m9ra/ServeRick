using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace ServeRick
{
    class WebMethod
    {
        public readonly MethodInfo Info;

        internal string Name { get { return Info.Name; } }

        internal WebMethod(MethodInfo info)
        {
            Info = info;
        }
    }
}
