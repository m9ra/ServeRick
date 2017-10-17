using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using ServeRick.Memory;
using System.Threading;

namespace ServeRick.Networking
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

        readonly int _bufferSize;

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
        internal Accepter(NetworkConfiguration configuration, MemoryConfiguration memoryConfiguration, BufferProvider bufferProvider, OnClientAccepted clientAcceptedHandler)
        {
            _configuration = configuration;
            _bufferProvider = bufferProvider;

            _bufferSize = memoryConfiguration.ClientBufferSize;

            _listener = new TcpListener(configuration.ListenAddress, configuration.ListenPort);
            _onClientAccepted = clientAcceptedHandler;
        }

        /// <summary>
        /// Run accepter in separate thread
        /// </summary>
        internal void Run()
        {
            _listener.Start(4096 * 4 * 2);
            var synchronizedAccepter = new Thread(() =>
            {
                for (;;)
                {
                    TcpClient clientSocket = null;
                    clientSocket = _listener.AcceptTcpClient();

                    Task.Run(() =>
                    {
                        IPEndPoint ep = null;
                        try
                        {
                            ep = clientSocket.Client.RemoteEndPoint as IPEndPoint;
                        }
                        catch (SocketException ex)
                        {
                            //Why on Earth we should suffer from exception when reading IP adress...
                            Log.Error($"IP reading with errorcode {ex.ErrorCode} of exception {ex}");
                        }

                        var buffer = _bufferProvider.GetBuffer();
                        var ip = ep == null ? null : ep.Address;
                        var client = new Client(clientSocket, ip, buffer);
                        _onClientAccepted(client);
                    });
                }
            });

            synchronizedAccepter.Start();
            //acceptClientAsync();
        }

        /// <summary>
        /// Begin accepting clients
        /// </summary>
        private void acceptClientAsync()
        {
            _listener.BeginAcceptTcpClient(_acceptClient, null);
        }

        /// <summary>
        /// Callback for accepting clients
        /// </summary>
        /// <param name="result">Callback result</param>
        private void _acceptClient(IAsyncResult result)
        {
            TcpClient clientSocket = null;
            try
            {
                clientSocket = _listener.EndAcceptTcpClient(result);
                clientSocket.SendBufferSize = 2 * _bufferSize;
            }
            catch (SocketException ex)
            {
                Log.Error("Accepter._accetClient {0}", ex);
            }

            if (clientSocket != null)
            {
                IPEndPoint ep = null;
                try
                {
                    ep = clientSocket.Client.RemoteEndPoint as IPEndPoint;
                }
                catch (SocketException ex)
                {
                    //Why on Earth we should suffer from exception when reading IP adress...
                    Log.Error($"IP reading with errorcode {ex.ErrorCode} of exception {ex}");
                }

                var buffer = _bufferProvider.GetBuffer();
                var ip = ep == null ? null : ep.Address;
                var client = new Client(clientSocket, ip, buffer);
                _onClientAccepted(client);
            }

            acceptClientAsync();
        }
    }
}
