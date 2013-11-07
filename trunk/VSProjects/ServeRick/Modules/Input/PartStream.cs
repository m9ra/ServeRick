using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Modules.Input
{
    public abstract class PartStream
    {
        protected ulong WrittenData { get; private set; }

        protected readonly ulong PartMaxSize;

        protected abstract bool write(byte[] data, int dataStart, int dataLength);


        protected PartStream(ulong partMaxSize)
        {
            PartMaxSize = partMaxSize;
        }


        internal bool Write(byte[] data, int dataStart, int dataLength)
        {
            WrittenData += (uint)dataLength;
            if (WrittenData > PartMaxSize)
            {
                Log.Error("Part written data exceeds limit {0}>{1}, at ({2},{3})", WrittenData, PartMaxSize, dataStart, dataLength);
                return false;
            }

            return write(data,dataStart,dataLength);
        }

        internal protected virtual void Completed()
        {
            //by default there is nothing to do
        }
    }
}
