using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Responsing;
using SharpServer.Networking;

namespace SharpServer
{
    internal delegate void ResponseHandler(Response response);

    /// <summary>
    /// This is just testing implementation that will be heavily changed
    /// </summary>
    class Response
    {
        private readonly Client _client;
        private readonly ResponseProcessor _processor;

        /// <summary>
        /// TODO this is for debug only
        /// </summary>
        StringBuilder _writtenData = new StringBuilder();

        Queue<byte[]> _toSend = new Queue<byte[]>();

        ResponseWorkItem _currentWork;

        public Response(Client client, ResponseProcessor processor)
        {
            _client = client;
            _processor = processor;
        }

        /// <summary>
        /// TODO will be changed to byte[] utf8 encoding
        /// </summary>
        /// <param name="data"></param>
        public void Write(byte[] data)
        {
            if (data == null || data.Length == 0)
                return;
            _toSend.Enqueue(data);
        }

        public string GetResult()
        {
            return _writtenData.ToString();
        }

        internal void RunWork(ResponseWorkItem work)
        {
            _currentWork = work;
            work.Handler(this);
            sendQueue();
        }

        /// <summary>
        /// Non blocking single threaded queue send
        /// </summary>
        private void sendQueue()
        {
            if (_toSend.Count == 0)
            {
                //there is no other work
                _client.Close();

                //TODO: there will be possible enqueuing of next work items
                return;
            }

            var data = _toSend.Dequeue();            
            _client.Send(data, sendQueue);
        }

        internal void Render(ResponseHandler page)
        {
            _currentWork = new ResponseWorkItem(_client, page);
            _processor.EnqueueWork(_currentWork);
        }
    }
}
