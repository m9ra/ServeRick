using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Memory;
using SharpServer.Networking;
using SharpServer.Responsing;

namespace SharpServer
{
    /// <summary>
    /// Accept, download request and create response via ReponseProcessors from/to clients
    /// </summary>
    class HttpServer
    {
        /// <summary>
        /// Accepter used for accepting clients
        /// </summary>
        readonly Accepter _accepter;

        /// <summary>
        /// Provides downloading requests
        /// </summary>
        readonly Downloader _downloader;

        /// <summary>
        /// Manager of available controllers
        /// </summary>
        readonly ControllerManager _controllers;

        /// <summary>
        /// TODO: There will be multiple processors
        /// </summary>
        readonly ResponseProcessor _processor;

        internal HttpServer(ControllerManager controllers, NetworkConfiguration networkConfiguration, MemoryConfiguration memoryConfiguration)
        {
            var provider = new BufferProvider(memoryConfiguration.ClientBufferSize, memoryConfiguration.MaximalClientMemoryUsage);

            _accepter = new Accepter(networkConfiguration, provider, _acceptClient);
            _downloader = new Downloader(_onHeadCompleted);
            _processor = new ResponseProcessor();
            _controllers = controllers;
        }

        /// <summary>
        /// Start server, listening on configured port
        /// </summary>
        internal void Start()
        {
            _accepter.Run();
        }

        /// <summary>
        /// Callback for accepting clients
        /// </summary>
        /// <param name="client">Accepted client</param>
        private void _acceptClient(Client client)
        {
            //TODO maybe register all clients, but disconnecting needs locking
            _downloader.DownloadHead(client);
        }

        /// <summary>
        /// Callback for on head completed event
        /// </summary>
        /// <param name="client">Client which head has been downloaded</param>
        private void _onHeadCompleted(Client client)
        {
            //Send to response processor
            //TODO selecting processor according to session data

            client.Response = new Response(client, _processor);
            _controllers.Handle(client);
        }
    }
}
