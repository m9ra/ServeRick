using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ServeRick.Processing;

namespace ServeRick.Responsing
{
    class WriteWorkItem : ClientWorkItem
    {
        private DataStream _data;

        internal WriteWorkItem(DataStream data)
        {
            _data = data;
        }

        protected override WorkProcessor getPlannedProcessor()
        {
            return Unit.Output;
        }

        internal override void Run()
        {
            Client.OnClose += close;
            sendHandler();
        }

        private void sendHandler()
        {
            Client.Response.Flush(onFlushed);
        }

        private void onFlushed()
        {
            _data.BeginRead(Client.Buffer.Storage, onDataAvailable);
        }

        private void onDataAvailable(byte[] buffer, int length)
        {
            if (length > 0)
            {
                Client.Send(buffer, sendHandler);
            }
            else
            {
                Complete();
            }
        }

        protected override void onComplete()
        {
            base.onComplete();
            close();
        }

        private void close()
        {
            if (_data == null)
                //already closed
                return;

            _data.Close();
        }
    }
}
