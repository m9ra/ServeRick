using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpServer.Networking
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
        
        /// <summary>
        /// Here are stored parsed headers
        /// </summary>
        private HttpRequest _request;

        #endregion

        /// <summary>
        /// Determine that all data has been recieved
        /// </summary>
        public bool IsComplete { get { return _request!=null && _request.IsComplete; } }

        /// <summary>
        /// Determine that all data for headers has been recieved
        /// </summary>
        public bool IsHeadComplete { get { return _request != null && _request.IsHeadComplete; } }
       
        /// <summary>
        /// Add data recieved from client. 
        /// </summary>
        /// <param name="buffer">Buffer which data will be added.</param>        
        public void AppendData(byte[] buffer, int inputLength)
        {
            if (buffer == null)
                //there is nothing to add
                return;

            int processedDataLength = 0;
            if (!IsHeadComplete)
            {
                //add needed data into head
                processedDataLength = appendDataToHead(buffer, inputLength);
            }

            //remaining data will be pasted into body
            if (processedDataLength < inputLength)
            {
                var dataLength=inputLength-processedDataLength;
                _request.AppendContentData(processedDataLength,dataLength, buffer);
            }
        }

        /// <summary>
        /// Get parsed request from recieved data
        /// </summary>
        /// <returns>Parsed request</returns>
        public HttpRequest GetRequest()
        {
            if (_request == null)
            {
                throw new NotSupportedException("Cannot get request until first header is recieved");
            }
            return _request;
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
                _request = new HttpRequest(toks[0], toks[1], toks[2]);
            }
            else
            {
                //usual headers
                _request.SetHeader(_name.ToString(), _value.ToString());
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
            _request.TryGetHeader("Content-Encoding", out encoding, "ascii");

            //resolve content length
            int contentLength = 0;
            string contentLengthString;
            if (_request.TryGetHeader("Content-length", out contentLengthString))
            {
                contentLength = int.Parse(contentLengthString);
            }

            _request.CompleteHead(encoding, contentLength);            
        }

        #endregion

        #region Processing head character input

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

            //ale given data readed into header
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
            if (_name.Length == 0 && _request!=null)
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
