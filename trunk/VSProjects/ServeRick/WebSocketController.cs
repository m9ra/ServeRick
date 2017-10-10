using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ServeRick.Networking;

namespace ServeRick
{
    public abstract class WebSocketController
    {
        private object _L_registered = new object();

        private readonly HashSet<WebSocket> _registeredSockets = new HashSet<WebSocket>();

        private readonly Thread _worker;

        protected abstract void ProcessMessage(WebSocket socket, string message);

        /// <summary>
        /// Non synchronized notification about connections/disconnections
        /// </summary>
        public event Action RegisteredSocketsChange;

        protected WebSocketController()
        {
            _worker = new Thread(WorkerThread)
            {
                IsBackground = true
            };
            _worker.Start();
        }

        protected static WebSocketField<T> NewField<T>()
        {
            return new WebSocketField<T>();
        }

        protected IEnumerable<WebSocket> GetRegisteredSockets()
        {
            lock (_L_registered)
                return _registeredSockets.ToArray();
        }

        protected virtual void WorkerThread()
        {
            //by default, there is no worker - the thread immediately closes.
        }

        internal void OnOpen(WebSocket webSocket)
        {
            lock (_L_registered)
            {
                if (!_registeredSockets.Add(webSocket))
                    return;
            }

            RegisteredSocketsChange?.Invoke();
        }

        internal void OnClose(WebSocket webSocket)
        {
            lock (_L_registered)
            {
                if (!_registeredSockets.Remove(webSocket))
                    return;
            }

            RegisteredSocketsChange?.Invoke();
            ProcessDisconnection(webSocket);
        }

        internal void MessageReceived(WebSocket socket, string message)
        {
            ProcessMessage(socket, message);
        }

        protected virtual void ProcessDisconnection(WebSocket socket)
        {
            //by default there is nothing to do
        }
    }
}
