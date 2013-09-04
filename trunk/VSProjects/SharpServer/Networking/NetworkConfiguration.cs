using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace SharpServer.Networking
{
    class NetworkConfiguration
    {
        internal readonly int ListenPort;
        internal readonly IPAddress ListenAddress;

        internal NetworkConfiguration(int listenPort, IPAddress listenAddress)
        {
            ListenPort = listenPort;
            ListenAddress = listenAddress;
        }
    }
}
