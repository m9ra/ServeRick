using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick
{
    public delegate void DataAvailableCallback(byte[] data, int length);

    public abstract class DataStream
    {
        protected abstract void beginRead(byte[] data, DataAvailableCallback callback);

        protected abstract void close();

        public void BeginRead(byte[] data, DataAvailableCallback callback)
        {
            beginRead(data, callback);
        }

        public void Close()
        {
            close();
        }
    }
}
