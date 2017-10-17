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

        private object _L_messages = new object();

        private readonly HashSet<WebSocket> _registeredSockets = new HashSet<WebSocket>();

        private readonly Thread _worker;

        private readonly Thread _connectionHandler;

        private readonly List<Action> _messages = new List<Action>();

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

            _connectionHandler = new Thread(ConnectionThread)
            {
                IsBackground = true
            };
            _connectionHandler.Start();
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

        private void ConnectionThread()
        {
            var localMessages = new List<Action>();
            while (true)
            {
                lock (_L_messages)
                {
                    Monitor.Wait(_L_messages);
                    localMessages.AddRange(_messages);
                    _messages.Clear();
                }

                foreach (var message in localMessages)
                {
                    message();
                }
                localMessages.Clear();
            }
        }

        internal void OnOpen(WebSocket webSocket)
        {
            addMessage(() =>
            {
                lock (_L_registered)
                {
                    if (!_registeredSockets.Add(webSocket))
                        return;
                }

                RegisteredSocketsChange?.Invoke();
            });
        }

        internal void OnClose(WebSocket webSocket)
        {
            addMessage(() =>
            {
                lock (_L_registered)
                {
                    if (!_registeredSockets.Remove(webSocket))
                        return;
                }

                RegisteredSocketsChange?.Invoke();
                ProcessDisconnection(webSocket);
            });
        }

        internal void MessageReceived(WebSocket socket, string message)
        {
            addMessage(() =>
            {
                ProcessMessage(socket, message);
            });
        }

        protected virtual void ProcessDisconnection(WebSocket socket)
        {
            //by default there is nothing to do
        }

        private void addMessage(Action action)
        {
            lock (_L_messages)
            {
                _messages.Add(action);
                Monitor.Pulse(_L_messages);
            }
        }
    }
}
