using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Responsing;
using ServeRick.Networking;

namespace ServeRick
{
    public delegate void ResponseHandler(Response response);

    /// <summary>
    /// This is just testing implementation that will be heavily changed
    /// </summary>
    public class Response
    {        
        private readonly ResponseProcessor _processor;
        private readonly Dictionary<string, ResponseHandler> _contentsFor = new Dictionary<string, ResponseHandler>();
        /// <summary>
        /// TODO strongly typed parameters
        /// </summary>
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly Queue<ResponseWorkItem> _workItems = new Queue<ResponseWorkItem>();
        protected readonly Dictionary<string, string> _responseHeaders = new Dictionary<string, string>();        

        private bool _headersSent = false;
        private string _statusLine = "HTTP/1.1 200 OK";

        protected Queue<byte[]> _toSend = new Queue<byte[]>();
        

        internal readonly Client Client;

        internal Response(Client client, ResponseProcessor processor)
        {
            Client = client;
            _processor = processor;

            _responseHeaders["Server"] = "ServeRick";

            _responseHeaders["Cache-Control"] = "max-age=0, private, must-revalidate";
            SetContentType("text/html; charset=utf-8"); //default content type
        }

        /// <summary>
        /// Allow Mocking responses creation
        /// </summary>
        protected Response()
        {
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

        public void Render(ResponseHandler handler)
        {
            var work = new ResponseWorkItem(Client, handler);
            _workItems.Enqueue(work);
        }

        public void Yield(string identifier)
        {
            ResponseHandler yieldHandler;
            if (_contentsFor.TryGetValue(identifier, out yieldHandler))
            {
                yieldHandler(this);
                return;
            }
        }

        

        internal void ContentFor(string yieldIdentifier, ResponseHandler handler)
        {
            _contentsFor[yieldIdentifier] = handler;
        }

        internal void RunWork(ResponseWorkItem work)
        {
            _workItems.Enqueue(work);
            while (_workItems.Count > 0)
            {
                var currentWork = _workItems.Dequeue();
                currentWork.Handler(this);
            }
            sendQueue();
        }

        internal void EnqueueToProcessor(ResponseHandler handler)
        {
            var work = new ResponseWorkItem(Client, handler);
            _processor.EnqueueWork(work);
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

            if (_toSend.Count == 0 && _workItems.Count == 0)
            {
                //there is no other work
                Client.Close();

                //TODO: there will be possible enqueuing of next work items
                return;
            }

            var data = _toSend.Dequeue();
            Client.Send(data, sendQueue);
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
            var bytes = Encoding.ASCII.GetBytes(builder.ToString());
            Client.Send(bytes, sendQueue);
        }

        internal void SetParam(string paramName, object paramValue)
        {
            _parameters[paramName] = paramValue;
        }

        public object GetParam(string paramName)
        {
            object result;
            _parameters.TryGetValue(paramName, out result);
            return result;
        }
    }
}
