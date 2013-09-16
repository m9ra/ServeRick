using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer
{
    class ActionInfo
    {
        internal readonly string Pattern;
        internal readonly ResponseHandler Handler;

        internal ActionInfo(string pattern, ResponseHandler handler)
        {
            Pattern = pattern;
            Handler = handler;
        }
    }
}
