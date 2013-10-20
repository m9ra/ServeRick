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
        /// <summary>
        /// Lock for thread saftynes of controll creation
        /// </summary>
        private object _L_controllers = new object();

        /// <summary>
        /// Creates controller for requests, containing content (POST) data. Controller
        /// is responsible for interpreting content data.
        /// <remarks>Is called synchronously</remarks>
        /// </summary>
        /// <param name="request">Requests wich data will be interpreted</param>
        /// <returns>Created contorller</returns>
        protected abstract InputController createController(HttpRequest request);

        /// <summary>
        /// TODO create better unique numbers
        /// There is problem with initialization with timestamp (making requests is getting uid closer)
        /// </summary>
        protected long _lastUID=DateTime.Now.Ticks;

        /// <summary>
        /// Creates controller for requests, containing content (POST) data. Controller
        /// is responsible for interpreting content data.
        /// </summary>
        /// <param name="request">Requests wich data will be interpreted</param>
        /// <returns>Created contorller</returns>
        internal InputController CreateController(Client client)
        {
            InputController controller;
            lock (_L_controllers)
            {
                controller= createController(client.Request);
                ++_lastUID;
                controller.InputUID = _lastUID;
            }

            controller.Client = client;
            return controller;
        }
    }
}
