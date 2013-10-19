using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Networking;

namespace SharpServer
{
    public abstract class InputController
    {
        internal Client Client;

        protected HttpRequest Request { get { return Client.Parser.Request; } }

        public bool ContinueDownloading { get; protected set; }

        protected abstract void acceptData(byte[] data, int dataOffset, int dataLength);

        protected InputController()
        {
            ContinueDownloading = true;
        }

        internal void AcceptData(byte[] data, int dataOffset, int dataLength)
        {
            acceptData(data, dataOffset, dataLength);
        }
    }
}
