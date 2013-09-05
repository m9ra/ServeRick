using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;

using SharpServer.Memory;

namespace SharpServer.Networking
{
    /// <summary>
    /// Delegate for handling events when client is successfully accepted
    /// </summary>
    /// <param name="client">Client that has been accepted</param>
    internal delegate void OnClientAccepted(Client client);

    /// <summary>
    /// Provides client accepting services for server
    /// </summary>
    class Accepter
    {
        /// <summary>
        /// Current configuration of network
        /// </summary>
        readonly NetworkConfiguration _configuration;

        /// <summary>
        /// Buffer provider for client communication
        /// </summary>
        readonly BufferProvider _bufferProvider;

        /// <summary>
        /// Listener used for client accepting
        /// </summary>
        readonly TcpListener _listener;

        /// <summary>
        /// Handler for accepted clients
        /// </summary>
        readonly OnClientAccepted _onClientAccepted;
        
        /// <summary>
        /// Create client accepter with given configuration.
        /// <remarks>Accepter uses buffer provider, for creating client buffers</remarks>
        /// </summary>
        /// <param name="configuration">Configuration of network</param>
        /// <param name="bufferProvider">Buffer provider for client communication</param>
        /// <param name="clientAcceptedHandler">Handler called on every accepted client. Is called synchronously</param>
        internal Accepter(NetworkConfiguration configuration, BufferProvider bufferProvider, OnClientAccepted clientAcceptedHandler)
        {
            _configuration = configuration;
            _bufferProvider = bufferProvider;

            _listener = new TcpListener(configuration.ListenAddress, configuration.ListenPort);
            _onClientAccepted = clientAcceptedHandler;
        }

        /// <summary>
        /// Run accepter in separate thread
        /// </summary>
        internal void Run()
        {
            _listener.Start(1024);
            acceptClient();
        }

        /// <summary>
        /// Begin accepting clients
        /// </summary>
        private void acceptClient()
        {
            _listener.BeginAcceptTcpClient(_acceptClient, null);            
        }

        /// <summary>
        /// Callback for accepting clients
        /// </summary>
        /// <param name="result">Callback result</param>
        private void _acceptClient(IAsyncResult result)
        {
            var clientSocket = _listener.EndAcceptTcpClient(result);
            var clientBuffer = _bufferProvider.GetBuffer();

            var client = new Client(clientSocket, clientBuffer);
            _onClientAccepted(client);
            acceptClient();
        }

    }
}
