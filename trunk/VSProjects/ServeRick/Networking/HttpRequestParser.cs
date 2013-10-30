using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;

namespace ServeRick.Networking
{
    /// <summary>
    /// Parse incoming data into header requests
    /// </summary>
    public class HttpRequestParser
    {

        #region Private members

        /// <summary>
        /// Storage for header name incrementing        
        /// </summary>
        private readonly StringBuilder _name = new StringBuilder();

        /// <summary>
        /// Storage for header value incrementing
        /// </summary>
        private readonly StringBuilder _value = new StringBuilder();

        /// <summary>
        /// Determine that we are reading header name (if it's true , or header value oterhwise).
        /// <remarks>First line has no header name, so we are not reading name when started</remarks>
        /// </summary>
        private bool _isReadingName = false;

        #endregion

        public readonly Regex SplitterGET = new Regex(@"
        (
            (?<Name> [^&=]+)
            =?
            (?<Value> [^&=]*)
        )*
        ", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public bool IsError { get; private set; }

        /// <summary>
        /// Determine that all data has been recieved
        /// </summary>
        public bool IsComplete { get { return IsError || (Request != null && Request.IsComplete); } }

        /// <summary>
        /// Determine that all data for headers has been recieved
        /// </summary>
        public bool IsHeadComplete { get { return Request != null && Request.IsHeadComplete; } }

        /// <summary>
        /// Parsed request
        /// </summary>
        public HttpRequest Request { get; private set; }

        /// <summary>
        /// How many bytes has been recieved
        /// </summary>
        public int RecievedBytes { get; private set; }

        /// <summary>
        /// Add data recieved from client. 
        /// </summary>
        /// <param name="buffer">Buffer which data will be added.</param>        
        /// <returns>Offset where non processed data starts</returns>
        public int AppendData(byte[] buffer, int inputLength)
        {
            if (buffer == null)
                //there is nothing to add
                return 0;

            RecievedBytes += inputLength;

            int processedDataLength = 0;
            if (!IsHeadComplete)
            {
                //add needed data into head
                processedDataLength = appendDataToHead(buffer, inputLength);
            }

            return processedDataLength;
        }

        #region Completitions handlers

        /// <summary>
        /// Called when single header is completed
        /// </summary>
        private void onHeaderComplete()
        {
            if (_name.Length == 0)
            {
                //first header doesn't have name
                var toks = _value.ToString().Split(' ');
                if (toks.Length != 3)
                {
                    IsError = true;
                    Log.Error("Invalid request with query {0}", _value);
                    return;
                }

                Request = createRequest(toks[0], toks[1], toks[2]);
            }
            else
            {
                //usual headers
                Request.SetHeader(_name.ToString(), _value.ToString());
            }

            _name.Clear();
            _value.Clear();
            _isReadingName = true;
        }

        /// <summary>
        /// Called when all headers are completed
        /// </summary>
        private void onHeadComplete()
        {
            //resolve encoding
            string encoding;
            Request.TryGetHeader("Content-Encoding", out encoding, "ascii");

            //resolve content length
            int contentLength = 0;
            string contentLengthString;
            if (Request.TryGetHeader("Content-length", out contentLengthString))
            {
                contentLength = int.Parse(contentLengthString);
            }

            Request.CompleteHead(encoding, contentLength);
        }

        #endregion

        #region Processing head character input

        private HttpRequest createRequest(string method, string requestUri, string version)
        {
            string uri;
            var getVariables = new Dictionary<string, string>();
            var dataStart = requestUri.IndexOf('?');
            if (dataStart <= 0)
            {
                //there are no GET data 
                uri = requestUri;
            }
            else
            {
                uri = requestUri.Substring(0, dataStart);
                var queryString = requestUri.Substring(dataStart + 1);
                fillGetVars(queryString, getVariables);
            }

            return new HttpRequest(method, uri, version, getVariables);
        }

        private void fillGetVars(string queryString, Dictionary<string, string> getVariables)
        {
            var match = SplitterGET.Match(queryString);
            if (!match.Success)
            {
                Log.Notice("Invalid query string");
                return;
            }

            var names = match.Groups["Name"].Captures;
            var values = match.Groups["Value"].Captures;
            for (int i = 0; i < names.Count; ++i)
            {
                var name = names[i].Value;
                var value = values[i].Value;

                getVariables[name] = value;
            }
        }

        private int appendDataToHead(byte[] buffer, int length)
        {
            //head has to have ascii encoding
            for (int i = 0; i < length; ++i)
            {
                var inputChar = (char)buffer[i];

                switch (inputChar)
                {
                    case '\r':
                        //skip carriage return
                        break;
                    case '\n':
                        newLineHeadChar();

                        if (IsHeadComplete)
                        {
                            //return index where body starts
                            return i + 1;
                        }

                        break;
                    case ' ':
                        whitespaceHeadChar();
                        break;
                    case ':':
                        colonHeadChar();
                        break;
                    default:
                        contentHeadChar(inputChar);
                        break;
                }

            }

            //ale given data read into header
            return length;
        }

        private void colonHeadChar()
        {
            if (_isReadingName)
            {
                //switch reading header name into reading header value
                _isReadingName = false;
            }
            else
            {
                //char is part of value
                _value.Append(':');
            }
        }

        private void whitespaceHeadChar()
        {
            if (!_isReadingName)
            {
                //headnames cannot contain whitespace

                if (_value.Length > 0)
                    //dont include trailing whitespace
                    _value.Append(' ');
            }
        }

        private void newLineHeadChar()
        {
            if (_name.Length == 0 && Request != null)
            {
                //end of headers section 
                //(request hasn't been created yet and last header doesn't have name)
                onHeadComplete();
            }
            else
            {
                //end of single header
                onHeaderComplete();
            }
        }

        private void contentHeadChar(char inputChar)
        {
            if (_isReadingName)
            {
                //we are reading header name
                _name.Append(inputChar);
            }
            else
            {
                //we are reding header value
                _value.Append(inputChar);
            }
        }
        #endregion
    }
}
