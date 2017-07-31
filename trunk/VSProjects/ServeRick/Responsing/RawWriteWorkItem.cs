using ServeRick.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Responsing
{
    class RawWriteWorkItem : ResponseWorkItem
    {
        private readonly Client _client;

        private readonly string _data;

        internal RawWriteWorkItem(Client client, string data)
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

            _client.Response.Flush(onFlushed);
        }

        private void onFlushed()
        {
            var bytes = Encoding.UTF8.GetBytes(_data);
            _client.Send(bytes, bytes.Length, sendHandler);
        }

        private void sendHandler()
        {
            Complete();
        }

        protected override void onAbort()
        {
            Complete();
        }

    }
}
