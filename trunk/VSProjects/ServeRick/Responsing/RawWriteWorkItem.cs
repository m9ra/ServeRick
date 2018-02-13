using ServeRick.Networking;
using ServeRick.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Responsing
{
    class RawWriteWorkItem : ResponseWorkItem, INetworkRecordable
    {
        private readonly Client _client;

        private readonly string _data;

        private readonly byte[] _dataBytes;

        internal RawWriteWorkItem(Client client, string data)
        {
            _client = client;
            _data = data;
        }

        internal RawWriteWorkItem(Client client, byte[] data)
        {
            _client = client;
            _dataBytes = data;
        }

        internal override void Run()
        {
            if (_client.IsClosed)
            {
                throw new InvalidOperationException("Cannot run, when client is closed");
            }

            _client.Response.Flush(onFlushed);
        }

        private void onFlushed()
        {
            var bytes = GetData();
            _client.Send(bytes, bytes.Length, sendHandler);
        }

        private void sendHandler()
        {
            Complete();
        }

        public byte[] GetData()
        {
            return _dataBytes ?? Encoding.UTF8.GetBytes(_data);
        }

        protected override void onAbort()
        {
            //there is nothing to do
        }
    }
}
