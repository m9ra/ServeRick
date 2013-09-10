using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Networking;

namespace SharpServer
{
    abstract class ControllerManager
    {
        internal abstract void Handle(Client client);
    }
}
