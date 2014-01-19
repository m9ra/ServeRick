using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using ServeRick.Processing;
using ServeRick.Responsing;
using ServeRick.Networking;
using ServeRick.Sessions;
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

        private bool _flipSession = false;
        private bool _headersSent = false;
        private bool _closed = false;
        private bool _resetContentHeader = false;
        private bool _canCache = false;

        private int _pendingSends = 0;

        internal event Action AfterSend;

        private string _statusLine;

        protected Queue<byte[]> _toSend = new Queue<byte[]>();

        internal readonly Client Client;

        internal Response(Client client)
        {
            Client = client;

            SetStatus(200);
            SetHeader("Server", "ServeRick");
            SetHeader("Connection", "Close");
            SetContentType("text/html; charset=utf-8"); //default content type
        }

        /// <summary>
        /// Allow Mocking responses creation
        /// </summary>
        protected Response()
        {
        }


        internal void SetStatus(int statusCode)
        {
            switch (statusCode)
            {
                case 200:
                    _statusLine = "HTTP/1.1 200 OK"; ;
                    break;
                case 302:
                    _statusLine = "HTTP/1.1 302 Found";
                    break;
                case 304:
                    _statusLine = "HTTP/1.1 304 Not Modified";
                    break;
                case 404:
                    _statusLine = "HTTP/1.1 404 Not Found";
                    break;
                default:
                    throw new NotImplementedException("Status code: " + statusCode);
            }
        }

        internal void AllowSessionFlip()
        {
            _flipSession = true;
        }

        public void SetContentType(string mime)
        {
            SetHeader("Content-Type", mime);
        }

        public void SetETag(string etag)
        {
            if (etag == null)
                return;

            SetHeader("ETag", etag);
        }

        public void SetContentDisposition(string disposition)
        {
            SetHeader("Content-Disposition", disposition);
        }

        public void SetLength(int contentLength)
        {
            SetHeader("Content-Length", contentLength.ToString());
        }

        [Obsolete("This is only quick workaround")]
        public void ResetContentLength()
        {
            _resetContentHeader = true;
        }

        public void CanCache()
        {
            _canCache = true;
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
        /// Get flash value for given messageID.
        /// </summary>
        /// <param name="messageID">ID of flash message</param>
        /// <returns>Flash message if present, null otherwise</returns>
        public string Flash(string messageID)
        {
            return SessionProvider.GetFlash(Client.Unit.Output, Client.SessionID, messageID);
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
                new ResponseHandlerWorkItem(Client, handler)
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

        internal void Flush(Action onFlushed)
        {
            AfterSend = onFlushed;

            sendQueue();
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
            Interlocked.Add(ref _pendingSends, _toSend.Count);

            if (!_headersSent)
            {
                Interlocked.Add(ref _pendingSends, 1);
                sendHeaders();
            }

            while (_toSend.Count > 0)
            {
                //send next data
                var data = _toSend.Dequeue();
                Client.Send(data, dataSended);
            }
        }

        private void dataSended()
        {
            var pending = Interlocked.Decrement(ref _pendingSends);

            if (pending == 0)
            {
                if (AfterSend != null)
                {
                    var callback = AfterSend;
                    AfterSend = null;
                    callback();
                }
            }
        }

        private void sendHeaders()
        {
            if (_headersSent)
                throw new NotSupportedException("Cannot send headers twice");

            if (_canCache)
            {
                SetHeader("Cache-Control", "max-age=3600, public, must-revalidate");
            }
            else
            {
                SetHeader("Cache-Control", "max-age=0, private, must-revalidate");
            }

            _headersSent = true;

            if (_resetContentHeader)
            {
                _responseHeaders.Remove("Content-Length");
            }

            var builder = new StringBuilder();
            builder.AppendLine(_statusLine);
            foreach (var pair in _responseHeaders)
            {
                builder.AppendFormat("{0}: {1}" + Environment.NewLine, pair.Key, pair.Value);
            }

            builder.AppendLine();
            var bytes = Encoding.ASCII.GetBytes(builder.ToString());
            Client.Send(bytes, dataSended);
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
            if (_closed)
            {
                throw new NotSupportedException("Cannot Close response twice");
            }

            _closed = true;

            sendQueue();

            if (_flipSession)
            {
                var flip = new FlipSessionWorkItem(Client.SessionID);
                Client.Unit.EnqueueIndependent(flip);
            }
        }
    }
}
