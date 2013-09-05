using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SharpServer.Networking;

namespace SharpServer.Responsing
{
    /// <summary>
    /// Item of work that has to be processed to completing client request
    /// <remarks>   
    ///     Usually there is multiple work items for single request. 
    ///     Also work items can be generated dynamically during request processing.
    /// </remarks>
    /// </summary>
    class ResponseWorkItem
    {
        /// <summary>
        /// Client which needs processing of this work item
        /// </summary>
        internal readonly Client Client;

        /// <summary>
        /// Handler that process given work item
        /// </summary>
        internal readonly ResponseHandler Handler;

        public ResponseWorkItem(Client client, ResponseHandler handler)
        {
            Client = client;
            Handler = handler;
        }
    }
}
