using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Networking;

namespace SharpServer
{
    public abstract class InputManagerBase
    {
        protected abstract InputController createController(HttpRequest request);

        private object _L_controllers = new object();

        internal InputController CreateController(Client client)
        {
            InputController controller;
            lock (_L_controllers)
            {
                controller= createController(client.Request);
            }

            controller.Client = client;

            return controller;
        }
    }
}
