using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace ServeRick.Networking
{
    /// <summary>
    /// Server configuration of network usage
    /// </summary>
    class NetworkConfiguration
    {
        /// <summary>
        /// Port where server is listening to incomming requests
        /// </summary>
        internal readonly int ListenPort;

        /// <summary>
        /// Address where server is listening to incomming requests
        /// </summary>
        internal readonly IPAddress ListenAddress;

        internal NetworkConfiguration(int listenPort, IPAddress listenAddress)
        {
            ListenPort = listenPort;
            ListenAddress = listenAddress;
        }
    }
}
