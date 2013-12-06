using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

namespace ServeRick
{
    public delegate void DataAvailableCallback(byte[] data, int length);

    public abstract class DataStream
    {
        private volatile bool _isClosed;

        private volatile bool _isReading;

        protected abstract void beginRead(byte[] data, DataAvailableCallback callback);

        protected abstract void close();

        public void BeginRead(byte[] data, DataAvailableCallback callback)
        {
            if (_isClosed)
                return;

            _isReading = true;
            beginRead(data, (d, l) => onDataRead(d, l, callback));
        }

        private void onDataRead(byte[] data, int length, DataAvailableCallback callback)
        {
            _isReading = false; 

            if (_isClosed)
                return;

            callback(data, length);
        }

        public void Close()
        {
            _isClosed = true;

            while (_isReading)
            {
                Thread.Sleep(1);
            }

            close();
        }
    }
}
