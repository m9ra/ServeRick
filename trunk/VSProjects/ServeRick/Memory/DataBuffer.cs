using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Memory
{
    class DataBuffer
    {
        /// <summary>
        /// Data storage for buffer
        /// </summary>
        readonly byte[] _bufferStorage;

        /// <summary>
        /// Buffer provider where bufferStorage will be recycled
        /// </summary>
        readonly BufferProvider _provider;

        /// <summary>
        /// Determine that buffer has already been recycled
        /// </summary>
        private bool _isRecycled = false;

        /// <summary>
        /// Storage of buffer, that can be used until buffer is recycled
        /// </summary>
        public byte[] Storage
        {
            get
            {
                if (_isRecycled)
                    throw new NotSupportedException("Cannot get storage when its recycled");

                return _bufferStorage;
            }
        }

        /// <summary>
        /// Create buffer of given length
        /// </summary>
        /// <param name="length">Length of buffer in bytes</param>
        /// <param name="provider">Provider where created buffer storage will be recycled</param>
        internal DataBuffer(int length, BufferProvider provider)
        {
            _bufferStorage = new byte[length];
            _provider = provider;
        }

        /// <summary>
        /// Create buffer from given bufferStorage
        /// </summary>
        /// <param name="bufferStorage">Storage for buffer</param>
        /// <param name="provider">Provider where buffer storage will be recycled</param>
        internal DataBuffer(byte[] bufferStorage, BufferProvider provider)
        {
            _bufferStorage = bufferStorage;
            _provider = provider;
        }

        /// <summary>
        /// Recycle buffer - dataStorage is returned to its buffer provider
        /// </summary>
        internal void Recycle()
        {
            if (_isRecycled)
            {
                throw new NotSupportedException("Recyclation cannot be processed twice. Possible incorrect client handling");
            }

            _provider.RecycleBufferStorage(_bufferStorage);
        }

    }
}
