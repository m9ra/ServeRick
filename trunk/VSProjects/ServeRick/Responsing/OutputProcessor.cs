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
        /// </summary>
        internal readonly Dictionary<string, SessionProvider> Sessions = new Dictionary<string, SessionProvider>();
    }
}
