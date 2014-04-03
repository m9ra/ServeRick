using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick.Networking;
using ServeRick.Processing;

namespace ServeRick.Responsing
{
    class WriteWorkItem : ResponseWorkItem
    {
        private readonly DataStream _data;

        private readonly Client _client;

        internal WriteWorkItem(Client client, DataStream data)
        {
            _client = client;
            _data = data;
        }

        internal override void Run()
        {
            if (_client.IsClosed)
            {
                throw new InvalidOperationException("Cannot run, when client is closed");
            }
            sendHandler();
        }

        private void sendHandler()
        {
            _client.Response.Flush(onFlushed);
        }

        private void onFlushed()
        {
            _data.BeginRead(_client.Buffer.Storage, onDataAvailable);
        }

        private void onDataAvailable(byte[] buffer, int length)
        {
            if (length > 0)
            {
                _client.Send(buffer, length, sendHandler);
            }
            else
            {
                close();
                Complete();
            }
        }

        protected override void onAbort()
        {
            close();
            Complete();
        }

        private void close()
        {
            _data.Close();
        }
    }
}
