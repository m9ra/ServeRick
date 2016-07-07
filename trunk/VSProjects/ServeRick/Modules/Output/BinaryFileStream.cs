using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace ServeRick.Modules.Output
{
    public class BinaryFileStream : DataStream
    {
        /// <summary>
        /// Length of stream
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Determine whether the stream is already closed
        /// </summary>
        private volatile bool _isClosed = false;

        /// <summary>
        /// Buffer where data will be read
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// Streem of the file if available.
        /// </summary>
        FileStream _stream;


        public BinaryFileStream(string path)
        {
            if (File.Exists(path))
            {
                _stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                Length = (int)_stream.Length;
            }
        }

        /// <inheritdoc/>
        protected override void beginRead(byte[] data, DataAvailableCallback callback)
        {
            _buffer = data;
            _stream.BeginRead(_buffer, 0, _buffer.Length, dataAvailable, callback);
        }

        /// <inheritdoc/>
        protected override void close()
        {
            _isClosed = true;
            _stream.Close();
        }

        /// <summary>
        /// Callback used when data from disk are available.
        /// </summary>
        /// <param name="result">Read result.</param>
        private void dataAvailable(IAsyncResult result)
        {
            if (_isClosed)
                throw new InvalidOperationException("receive data on closed stream");

            var length = _stream.EndRead(result);
            var callback = result.AsyncState as DataAvailableCallback;

            callback(_buffer, length);
        }
    }
}
