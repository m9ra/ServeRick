using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;
using ServeRick.Responsing;
using ServeRick.Networking;
using ServeRick.Database;



namespace ServeRick
{
    public delegate void ResponseHandler(Response response);

    /// <summary>
    /// This is just testing implementation that will be heavily changed
    /// </summary>
    public class Response
    {
        /// <summary>
        /// TODO strongly typed parameters
        /// </summary>
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        protected readonly Dictionary<string, string> _responseHeaders = new Dictionary<string, string>();
        private readonly Dictionary<string, ResponseHandler> _contentsFor = new Dictionary<string, ResponseHandler>();

        private bool _headersSent = false;
        private bool _closeAfterSend = false;
        private string _statusLine = "HTTP/1.1 200 OK";

        protected Queue<byte[]> _toSend = new Queue<byte[]>();

        internal readonly Client Client;

        internal Response(Client client)
        {
            Client = client;

            SetHeader("Server", "ServeRick");
            SetHeader("Cache-Control", "max-age=0, private, must-revalidate");
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
            SetHeader("Content-Type", mime);
        }

        public void SetLength(int contentLength)
        {
            SetHeader("Content-Length", contentLength.ToString());
        }


        internal void SetHeader(string header, string value)
        {
            _responseHeaders[header] = value;
        }

        internal void SetCookie(string cookieName, string cookieValue)
        {
            SetHeader("Set-Cookie", cookieName + "=" + cookieValue);
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
            Client.EnqueueWork(
                new ResponseHandlerWorkItem(handler)
                );
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
                //nothing more to send
                if (_closeAfterSend)
                    //there is no other work
                    Client.Close();

                return;
            }

            //send next data
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

        internal void Close()
        {
            _closeAfterSend = true;
            sendQueue();
        }

    }
}
