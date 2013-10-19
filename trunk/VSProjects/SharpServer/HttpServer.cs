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
        /// Manager of available response controllers
        /// </summary>
        readonly ResponseManagerBase _responseManager;

        /// <summary>
        /// Manager of available input controllers
        /// </summary>
        readonly InputManagerBase _inputManager;

        /// <summary>
        /// TODO: There will be multiple processors
        /// </summary>
        readonly ResponseProcessor _responseProcessor;

        readonly InputProcessor _inputProcessor;

        internal HttpServer(WebApplication application, NetworkConfiguration networkConfiguration, MemoryConfiguration memoryConfiguration)
        {
            var provider = new BufferProvider(memoryConfiguration.ClientBufferSize, memoryConfiguration.MaximalClientMemoryUsage);

            _accepter = new Accepter(networkConfiguration, provider, _acceptClient);
            _downloader = new Downloader(_onHeadCompleted,_onContentCompleted);
            _responseProcessor = new ResponseProcessor();
            _responseManager = application.CreateResponseManager();
            _inputManager = application.CreateInputManager();
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
            Log.Trace("HttpServer._onClientAccept {0}", client);
            _downloader.DownloadHead(client);
        }

        /// <summary>
        /// Callback for on head completed event
        /// </summary>
        /// <param name="client">Client which head has been downloaded</param>
        private void _onHeadCompleted(Client client, byte[] data, int dataOffset, int dataLength)
        {            
            Log.Trace("HttpServer._onHeadCompleted {0}", client);

            if (client.Request.ContentLength > 0)
            {
                //Send to input processor
                var inputController = _inputManager.CreateController(client);
                client.Input = inputController;
                inputController.AcceptData(data, dataOffset, dataLength);

                if (inputController.ContinueDownloading)
                {
                    _downloader.DownloadContent(client);
                }
                else
                {
                    _onContentCompleted(client);
                }
            }
            else
            {
                //Send to response processor
                _onRequestCompleted(client);
            }
        }

        private void _onContentCompleted(Client client){
            _onRequestCompleted(client);
        }

        /// <summary>
        /// Callback for on request completed event
        /// </summary>
        /// <param name="client"></param>
        private void _onRequestCompleted(Client client)
        {
            //TODO selecting processor according to session data

            client.Response = new Response(client, _responseProcessor);
            _responseManager.Handle(client);
        }
    }
}
