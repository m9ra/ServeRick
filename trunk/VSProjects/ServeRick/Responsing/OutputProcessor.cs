using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using ServeRick.Processing;
using ServeRick.Networking;

namespace ServeRick.Responsing
{
    class OutputProcessor : WorkProcessor
    {
        /// <summary>
        /// NOTE: accessing can be done only from processed work items
        /// TODO: Give better contract to make sessions persistant
        /// </summary>
        internal readonly Dictionary<string, object> Sessions = new Dictionary<string, object>();
    }
}
