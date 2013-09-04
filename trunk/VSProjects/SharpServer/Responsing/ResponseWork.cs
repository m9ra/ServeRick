using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SharpServer.Networking;

namespace SharpServer.Responsing
{
    class ResponseWork
    {
        internal readonly Client Client;
        internal readonly ResponseHandler Handler;

        internal ResponseWork NextWork { get; private set; }

        public ResponseWork(Client client, ResponseHandler handler, ResponseWork nextWork=null)
        {            
            Client = client;
            Handler = handler;

            NextWork = nextWork;
        }
    }
}
