using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace SharpServer.Networking
{
    /// <summary>
    /// Request from parsed client data.
    /// <remarks>
    ///     Can handle requests with incomplete body (
    /// </remarks>
    /// </summary>
    public class HttpRequest
    {
        #region Private members

        /// <summary>
        /// Storage for recieved headers
        /// </summary>
        private Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Counter for bytes that has been recieved as content
        /// </summary>
        private int _recivedContentBytes;

        #endregion

        #region Main HTTP request headers
        /// <summary>
        /// Request method in upper case
        /// </summary>
        public readonly string Method;

        /// <summary>
        /// Request Uri with query string
        /// </summary>
        public readonly string URI;

        /// <summary>
        /// Http version with upper case
        /// </summary>
        public readonly string HttpVersion;

        /// <summary>
        /// Encoding detected for request
        /// </summary>
        public string ContentEncoding { get; private set; }

        /// <summary>
        /// Content length detected for request
        /// </summary>
        public int ContentLength { get; private set; } 
        #endregion

        #region Request state members

        /// <summary>
        /// Determine that request was completely recieved (including content bytes)
        /// </summary>
        public bool IsComplete { get { return IsHeadComplete && _recivedContentBytes >= ContentLength; } }

        /// <summary>
        /// Determine that headers was completely recieved 
        /// </summary>
        public bool IsHeadComplete { get; private set; }

        /// <summary>
        /// Determine that request contains error (probably in format, size limits,..)
        /// </summary>
        public bool ContainsError { get; private set; }

        #endregion

        /// <summary>
        /// Creates http request of given method, uri and version of http protocol
        /// </summary>
        /// <param name="method">Method for request (POST, GET,..)</param>
        /// <param name="uri">Uri with query string</param>
        /// <param name="httpVersion">Version of http</param>
        internal HttpRequest(string method, string uri, string httpVersion)
        {            
            Method = method.ToUpper();
            URI = uri;
            HttpVersion = httpVersion.ToUpper();
        }

        /// <summary>
        /// Try get header with given name
        /// </summary>
        /// <param name="headerName">Name of header</param>
        /// <param name="headerValue">Output value of header, if header is present. Default value otherwise</param>
        /// <param name="defaultValue">Default value for headerValue. Is used when header with given name is not found</param>
        /// <returns>True if header is found, false otherwise</returns>
        public bool TryGetHeader(string headerName, out string headerValue, string defaultValue=null)
        {
            if (_headers.TryGetValue(headerName, out headerValue))
            {
                return true;
            }

            headerValue = defaultValue;
            return false;   
        }

        #region Internal API for building request

        /// <summary>
        /// Append data which belongs to content
        /// </summary>
        /// <param name="startOffset">Start offset in given buffer, where appended data are stored</param>
        /// <param name="dataLength">Length of appended data</param>
        /// <param name="buffer">Buffer with appended data</param>
        internal void AppendContentData(int startOffset, int dataLength, byte[] buffer)
        {
            Debug.Assert(IsHeadComplete);

            _recivedContentBytes += dataLength;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set header with its value for this request
        /// </summary>
        /// <param name="headerName">Name of setted header</param>
        /// <param name="headerValue">Value of setted header</param>
        internal void SetHeader(string headerName, string headerValue)
        {
            Debug.Assert(!IsHeadComplete);

            _headers[headerName] = headerValue;
        }

        /// <summary>
        /// Completes header building for this request
        /// </summary>
        /// <param name="contentEncoding">Encoding for content</param>
        /// <param name="contentLength">Length of content</param>
        internal void CompleteHead(string contentEncoding, int contentLength)
        {
            Debug.Assert(!IsHeadComplete);

            ContentEncoding = contentEncoding;
            ContentLength = contentLength;
            IsHeadComplete = true;
        }

        #endregion
    }
}
