﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Memory;
using ServeRick.Database;
using ServeRick.Networking;
using ServeRick.Responsing;
using ServeRick.Processing;
using ServeRick.Sessions;

namespace ServeRick
{
    /// <summary>
    /// Accept, download request and create response via ReponseProcessors from/to clients
    /// </summary>
    public class HttpServer
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

        readonly List<BackgroundTask> _tasks = new List<BackgroundTask>();

        internal readonly ProcessingUnit Unit;

        public readonly BufferProvider BufferProvider;

        internal HttpServer(WebApplication application, NetworkConfiguration networkConfiguration, MemoryConfiguration memoryConfiguration)
        {
            BufferProvider = new BufferProvider(memoryConfiguration.ClientBufferSize, memoryConfiguration.MaximalClientMemoryUsage);

            _accepter = new Accepter(networkConfiguration, memoryConfiguration, BufferProvider, _acceptClient);
            _downloader = new Downloader(_onHeadCompleted, _onContentCompleted);
            _responseManager = application.CreateResponseManager();
            _inputManager = application.CreateInputManager();

            Unit = new ProcessingUnit();

            foreach (var table in application.CreateTables())
            {
                Unit.Database.AddTable(table);
            }
        }

        /// <summary>
        /// Runs background task of the server.
        /// </summary>
        public void RunTask(BackgroundTask task)
        {
            _tasks.Add(task);
            task.Run(this);
        }

        /// <summary>
        /// Start server, listening on configured port
        /// </summary>
        internal void Start()
        {
            foreach (var task in _tasks)
                task.WaitUntilInitialized();

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


            //TODO selecting processor according to session data
            client.SetUnit(Unit);
            client.Response = new Response(client);
            SessionProvider.PrepareSessionID(client);

            //resolve IP address accordingly forward headers
            string ipValue;
            var defaultIP = client.IP != null ? client.IP.ToString() : "none";
            client.Request.TryGetHeader(HttpRequest.ForwardedHeader, out ipValue, defaultIP);
            client.Request.SetHeader(HttpRequest.IPHeader, ipValue);

            if (client.Request.ContentLength > 0)
            {
                //Send to input processor
                var inputController = _inputManager.CreateController(client);
                if (inputController == null)
                {
                    //there is no input controller for client
                    _onContentCompleted(client);
                    return;
                }

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

        private void _onContentCompleted(Client client)
        {
            if (client.Input != null)
                client.Input.OnDownloadCompleted();

            _onRequestCompleted(client);
        }

        /// <summary>
        /// Callback for on request completed event
        /// </summary>
        /// <param name="client"></param>
        private void _onRequestCompleted(Client client)
        {
            _responseManager.Handle(client);
        }
    }
}
