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
        internal readonly WebItem Item;

        internal ActionInfo(string pattern, WebItem handler)
        {
            Pattern = pattern;
            Item = handler;
        }
    }
}
