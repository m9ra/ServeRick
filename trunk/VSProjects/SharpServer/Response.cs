using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Responsing;
using SharpServer.Networking;

namespace SharpServer
{
    public delegate void ResponseHandler(Response response);

    /// <summary>
    /// This is just testing implementation that will be heavily changed
    /// </summary>
    public class Response
    {
        private readonly Client _client;
        private readonly ResponseProcessor _processor;


        private bool _headersSent = false;
        private string _statusLine = "HTTP/1.1 200 OK";
        private readonly Dictionary<string, string> _responseHeaders=new Dictionary<string,string>();

        /// <summary>
        /// TODO this is for debug only
        /// </summary>
        StringBuilder _writtenData = new StringBuilder();

        Queue<byte[]> _toSend = new Queue<byte[]>();

        ResponseWorkItem _currentWork;

        internal Response(Client client, ResponseProcessor processor)
        {
            _client = client;
            _processor = processor;

            _responseHeaders["Server"] = "SharpServer";
            
            _responseHeaders["Cache-Control"] = "max-age=0, private, must-revalidate";
            SetContentType("text/html; charset=utf-8"); //default content type
        }

        public void SetContentType(string mime)
        {
            _responseHeaders["Content-Type"] = mime;
        }

        public void SetLength(int contentLength)
        {
            _responseHeaders["Content-Length"] = contentLength.ToString();
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

        internal void RunWork(ResponseWorkItem work)
        {
            _currentWork = work;
            work.Handler(this);
            sendQueue();
        }

        public void Render(ResponseHandler page)
        {
            _currentWork = new ResponseWorkItem(_client, page);
            _processor.EnqueueWork(_currentWork);
        }

        /// <summary>
        /// Non blocking single threaded queue send
        /// </summary>
        private void sendQueue()
        {
            if (!_headersSent)
            {
                sendHeaders();
                return;
            }

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

        private void sendHeaders()
        {
            _headersSent = true;

            var builder = new StringBuilder();
            builder.AppendLine(_statusLine);
            foreach (var pair in _responseHeaders)
            {
                builder.AppendFormat("{0}: {1}" + Environment.NewLine, pair.Key, pair.Value);
            }

            builder.AppendLine();
            var bytes=Encoding.ASCII.GetBytes(builder.ToString());
            _client.Send(bytes, sendQueue);
        }


    }
}
