using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Networking
{
    /// <summary>
    /// Delegate used for events when head downloading is completed
    /// </summary>
    /// <param name="client">Client which head downloading is completed</param>
    delegate void OnHeadCompleted(Client client, byte[] buffer, int dataOffset, int dataLength);

    delegate void OnContentDownloaded(Client client);

    /// <summary>
    /// Provides download services on client requests
    /// </summary>
    class Downloader
    {
        /// <summary>
        /// Callback for head completition event
        /// </summary>
        readonly OnHeadCompleted _onHeadCompleted;

        readonly OnContentDownloaded _onContentDownloaded;

        internal Downloader(OnHeadCompleted headCompleted, OnContentDownloaded contentDownloaded)
        {
            _onHeadCompleted = headCompleted;
            _onContentDownloaded = contentDownloaded;
        }

        /// <summary>
        /// Download head for given client
        /// </summary>
        /// <param name="client">Client which head will be downloaded</param>
        internal void DownloadHead(Client client)
        {
            client.Recieve(_processHead);
        }

        internal void DownloadContent(Client client)
        {
            client.Recieve(_processContent);
        }

        /// <summary>
        /// Callback for head downloading
        /// </summary>
        /// <param name="client">Client which head is processed</param>
        /// <param name="data">Data downloaded for head processing</param>
        /// <param name="dataLength">Length of recieved data</param>
        private void _processHead(Client client, byte[] data, int dataLength)
        {
            Log.Trace("Downloader._processHead client: {0}, dataLength: {1}", client, dataLength);

            var notProcessedOffset = client.Parser.AppendData(data, dataLength);
            if (client.Parser.IsHeadComplete)
            {
                var remainingLength = dataLength - notProcessedOffset;
                _onHeadCompleted(client, data, notProcessedOffset, remainingLength);
                return;
            }

            if (dataLength == 0)
            {
                Log.Notice("Downloader._onHeadCompleted {0},  recieved {1}B, incomplete header: {2}B", client, dataLength, client.Parser.RecievedBytes);
                client.Close();
                return;
            }

            DownloadHead(client);
        }

        private void _processContent(Client client, byte[] data, int dataLength)
        {
            Log.Trace("Downloader._processContent client: {0}, dataLength: {1}", client, dataLength);

            var input = client.Input;

            input.AcceptData(data, 0, dataLength);
            if (input.ContinueDownloading && dataLength > 0)
            {
                client.Recieve(_processContent);
            }
            else
            {
                _onContentDownloaded(client);
            }
        }
    }
}
