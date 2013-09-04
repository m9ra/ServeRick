using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Networking
{
    delegate void OnHeadCompleted(Client client);

    /// <summary>
    /// Provides download services on clients
    /// </summary>
    class Downloader
    {
        readonly OnHeadCompleted _onHeadCompleted;
        readonly NetworkConfiguration _configuration;

        internal Downloader(NetworkConfiguration configuration, OnHeadCompleted headCompleted)
        {
            _onHeadCompleted = headCompleted;
            _configuration = configuration;
        }

        internal void DownloadHead(Client client)
        {
            client.Recieve(processHead);
        }

        private void processHead(Client client,byte[] data,int dataLength)
        {
            client.Parser.AppendData(data, dataLength);
            if (client.Parser.IsHeadComplete)
            {
                _onHeadCompleted(client);
                return;
            }

            DownloadHead(client);
        }
    }
}
