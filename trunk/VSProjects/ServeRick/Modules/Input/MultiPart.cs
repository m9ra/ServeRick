using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace ServeRick.Modules.Input
{
    /// <summary>
    /// Content dispositions parsed for multi part stream.
    /// </summary>
    public class Dispositions : Dictionary<string, string> { }

    /// <summary>
    /// Class providing multi part content processing.
    /// </summary>
    public abstract class MultiPart : InputController
    {
        /// <summary>
        /// Parse content disposition entries        
        /// <example>form-data; name="uploadedFile"; filename=""</example>
        /// </summary>
        private static readonly Regex ContentDispositionParser = new Regex(@"
(?<DispositionType> \w[\w-]*);\s*
(?<DispositionParam> 
    (?<Name> \w[\w-]*)
    \s* = \s*

    (
        ""(?<Value> ([^""]|\\.)*)""
    |
        (?<Value> [^""]+)
    )

    \s* ;? \s*
)+
", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Boundary used for delimiting multipart sections
        /// </summary>
        private readonly Boundary _boundary;

        /// <summary>
        /// Determine that starting boundary has appeared yet
        /// </summary>
        private bool _hasStartingBoundary = false;

        /// <summary>
        /// Part stream of currently processed part
        /// </summary>
        private PartStream _currentPartStream;

        /// <summary>
        /// Buffer for dispositions keys/values
        /// </summary>
        private StringBuilder _buffer = new StringBuilder(100);

        /// <summary>
        /// Dispositions for current part. Null means that disposition section is not started.
        /// </summary>
        private Dispositions _dispositions;

        /// <summary>
        /// Stored disposition key for current dispositions section
        /// </summary>
        private string _dispositionKey;

        /// <summary>
        /// Report all part entries. Whole part content will be
        /// written into returned PartStream.
        /// </summary>
        /// <param name="dispositions">Content dispositions parsed for reported.</param>
        /// <returns>PartStream used for writing part stream.</returns>
        protected abstract PartStream reportPart(Dispositions dispositions);

        /// <summary>
        /// Count all recieved bytes
        /// </summary>
        public long TotalRecievedBytes { get; private set; }

        /// <summary>
        /// Create input controller for handling multipart data content
        /// </summary>
        /// <param name="boundary">Boundary used for multipart delimiting</param>
        public MultiPart(string boundary)
        {
            _boundary = new Boundary(boundary);
        }

        protected override void acceptData(byte[] data, int dataOffset, int dataLength)
        {
            TotalRecievedBytes += dataLength;

            var localContext = dataOffset;
            if (!_hasStartingBoundary)
            {
                localContext = getStartingBoundary(data, dataOffset, dataLength);
                if (!ContinueDownloading || !_hasStartingBoundary)
                    return;

                //find next boundary to delimit content
                _boundary.AcceptNext(data, dataOffset, dataLength);
            }
            else
            {
                //First boundary can't keep unsended content

                var lastCursor = _boundary.Cursor;
                //Try to find next boundary
                _boundary.AcceptNext(data, dataOffset, dataLength);


                if (lastCursor > 0 && _boundary.LocalStartOffset >= 0)
                {
                    //Data that were not processed in last buffer because
                    //of possible boundary start.
                    reportPartContent(_boundary.DelimiterData, 0, lastCursor);
                }
            }


            while (ContinueDownloading && localContext < dataOffset + dataLength)
            {
                if (_boundary.IsComplete)
                {
                    //Report data from current offset to boundary start
                    reportPartContent(data, localContext, _boundary.LocalStartOffset - localContext);
                    if (!ContinueDownloading)
                        //processing could be stopped because of error
                        break;
                    reportPartEnd();

                    localContext = _boundary.LocalEndOffset + 1;
                    _boundary.AcceptNext(data, localContext, dataLength + dataOffset - localContext);
                }
                else
                {
                    //whole data till end without data under cursor belongs to partcontent
                    var length = dataOffset + dataLength - localContext - _boundary.Cursor;
                    reportPartContent(data, localContext, length);
                    //all possible data were reported
                    break;
                }
            }
        }

        private int getStartingBoundary(byte[] data, int dataOffset, int dataLength)
        {
            _boundary.AcceptNext(data, dataOffset, dataLength);
            if (_boundary.IsComplete)
            {
                _hasStartingBoundary = true;
                return _boundary.LocalEndOffset + 1;
            }
            else
            {
                if (TotalRecievedBytes > _boundary.DelimiterData.Length)
                {
                    Log.Error("Expecting boundary at begining");
                    ContinueDownloading = false;
                }
            }
            return dataOffset;
        }

        private void reportPartEnd()
        {
            if (_currentPartStream == null)
            {
                Log.Error("Part end without part start");
                ContinueDownloading = false;
                return;
            }

            //report completition action
            _currentPartStream.Completed();
            //waiting for new stream
            _currentPartStream = null;
        }

        private void reportPartContent(byte[] data, int dataOffset, int dataLength)
        {
            var nextOffset = dataOffset;
            if (_currentPartStream == null)
            {
                nextOffset = reportHeaderContent(data, nextOffset, dataLength);
            }

            var remainingLength = dataOffset + dataLength - nextOffset;
            if (remainingLength > 0 && ContinueDownloading)
            {
                //NOTE: if something remains, header is complete and therefore part stream is set
                if (!_currentPartStream.Write(data, nextOffset, remainingLength))
                {
                    //part has been rejected
                    ContinueDownloading = false;
                }
            }
        }

        private void onSectionComplete()
        {
            var contentDisposition = _dispositions["Content-Disposition"];
            var match = ContentDispositionParser.Match(contentDisposition);
            if (!match.Success)
            {
                Log.Error("Content disposition cannot be matched: '{0}'", contentDisposition);
                return;
            }

            var names = match.Groups["Name"];
            var values = match.Groups["Value"];
            for (int i = 0; i < names.Captures.Count; ++i)
            {
                var name = names.Captures[i].Value;
                var value = values.Captures[i].Value;

                _dispositions[name] = value;
            }

            _currentPartStream = reportPart(_dispositions);
        }

        private int reportHeaderContent(byte[] data, int dataOffset, int dataLength)
        {
            int i;
            for (i = dataOffset; i < dataOffset + dataLength; ++i)
            {
                var ch = (char)data[i];

                switch (ch)
                {
                    case '\r':
                        //no action
                        break;

                    case ':':
                        //key confirmation
                        _dispositionKey = _buffer.ToString();
                        _buffer.Clear();

                        break;

                    case '\n':
                        //buffer confirmation

                        var value = _buffer.ToString();
                        _buffer.Clear();

                        if (_dispositions == null)
                        {
                            //disposition section hasn't been started yet

                            if (value == "--")
                            {
                                //end of last part
                                ContinueDownloading = false;
                                return int.MaxValue;
                            }

                            if (value == "")
                            {
                                //open disposition section
                                _dispositions = new Dispositions();
                                continue;
                            }
                            Log.Error("Unexpected dispositions key: {0}", value);
                            ContinueDownloading = false;
                            return int.MaxValue;
                        }

                        if (_dispositionKey == null)
                        {
                            //no key in open dispositions section means end of section
                            onSectionComplete();
                            if (_currentPartStream == null)
                                ContinueDownloading = false;

                            _dispositions = null;
                            return i + 1;
                        }

                        _dispositions[_dispositionKey] = value;
                        //reset key
                        _dispositionKey = null;
                        break;

                    default:
                        if (_buffer.Length == 0 && char.IsWhiteSpace(ch))
                            //remove trailing white chars
                            break;

                        _buffer.Append(ch);
                        break;
                }
            }

            if (_dispositions == null)
            {
                if (_buffer.Length == 2 && _buffer.ToString() == "--")
                {
                    //end of last part
                    ContinueDownloading = false;
                    return i;
                }
            }

            return i;
        }
    }
}
