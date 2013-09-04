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
    class HttpServer
    {
        readonly Accepter _accepter;
        readonly Downloader _downloader;

        /// <summary>
        /// TODO:There will be multiple processors
        /// </summary>
        readonly ResponseProcessor _processor;

        internal HttpServer(NetworkConfiguration configuration,ResponseHandler outOfServicePage)
        {
            var provider = new BufferProvider(4096, 10000000);
            _accepter = new Accepter(configuration, provider, acceptClient);
            _downloader = new Downloader(configuration, onHeadCompleted);
            _processor = new ResponseProcessor(outOfServicePage);
        }

        internal void Start()
        {
            _accepter.Run();
        }

        private void acceptClient(Client client)
        {
            //TODO maybe register all clients, but disconnecting needs locking
            _downloader.DownloadHead(client);
        }

        private void onHeadCompleted(Client client)
        {
            //Send to response processor
            //TODO selecting processor according to session data

            _processor.MakeResponse(client);
        }
    }
}
