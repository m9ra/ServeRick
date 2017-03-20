using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Networking;

namespace ServeRick
{
    public abstract class WebSocketController
    {
        private object _L_receive = new object();

        protected abstract void ProcessMessage(WebSocket socket, string message);

        internal void OnOpen(WebSocket webSocket)
        {
            //TODO: keep list of active sockets
        }

        internal void OnClose(WebSocket webSocket)
        {
            //TODO keep list of active clients
        }

        internal void MessageReceived(WebSocket socket, string message)
        {
            lock (_L_receive)
            {
                ProcessMessage(socket, message);
            }
        }
    }
}
