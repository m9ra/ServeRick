using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Memory
{
    /// <summary>
    /// Manage buffers creating and recycling
    /// <remarks>IS THREAD SAFE</remarks>
    /// </summary>
    class BufferProvider
    {
        /// <summary>
        /// Stack is used for better cache hits
        /// </summary>
        readonly Stack<DataBuffer> _freeBuffers = new Stack<DataBuffer>();

        /// <summary>
        /// Lock for free buffers stack
        /// </summary>
        readonly object _L_freeBuffers = new object();

        /// <summary>
        /// Length of created buffers
        /// </summary>
        readonly int _bufferLength;

        /// <summary>
        /// Maximal memory, that can be allocated for buffers
        /// </summary>
        readonly int _maximalMemoryUsage;

        /// <summary>
        /// Current
        /// </summary>
        int _currentMemoryUsage;

        /// <summary>
        /// Create provider of buffers with given length. Maximal memory usage is cheked.
        /// </summary>
        /// <param name="bufferLength">Length of created buffers in bytes</param>
        /// <param name="maximalMemoryUsage">Maximal memory usage in bytes</param>
        internal BufferProvider(int bufferLength, int maximalMemoryUsage)
        {
            if (bufferLength < 1)
                throw new NotSupportedException("Cannot create buffer with given length");

            if (maximalMemoryUsage < 1)
                throw new NotSupportedException("Needs more memory to use");

            _bufferLength = bufferLength;
            _maximalMemoryUsage = maximalMemoryUsage;
        }

        /// <summary>
        /// Recycle buffer storage, so it can be used repeatedly
        /// <remarks>It intented to be used from Buffer.Recycle() method</remarks>
        /// </summary>
        /// <param name="bufferStorage">Storage that is recycled from buffer</param>
        internal void RecycleBufferStorage(byte[] bufferStorage)
        {
            var newBuffer = new DataBuffer(bufferStorage, this);

            lock (_L_freeBuffers)
            {
                _freeBuffers.Push(newBuffer);
            }
        }

        /// <summary>
        /// Get buffer from manager
        /// </summary>
        /// <returns>Memory buffer</returns>
        internal DataBuffer GetBuffer()
        {
            lock (_L_freeBuffers)
            {
                if (_freeBuffers.Count > 0)
                {
                    //there is free buffer already allocated
                    return _freeBuffers.Pop();
                }
            }

            //we need to create new buffer
            return allocateBuffer();
        }

        /// <summary>
        /// Creates buffer with memory usage checking
        /// </summary>
        /// <returns>Created buffer</returns>
        private DataBuffer allocateBuffer()
        {
            if (_currentMemoryUsage + _bufferLength > _maximalMemoryUsage)
            {
                throw new NotImplementedException("Strategies for PANIC situation");
            }

            _currentMemoryUsage += _bufferLength;
            return new DataBuffer(_bufferLength, this);
        }

    }
}
