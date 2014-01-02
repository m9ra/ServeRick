using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;
using ServeRick.Networking;

namespace ServeRick.Responsing
{
    /// <summary>
    /// Item of work that has to be processed to completing client request.
    /// <remarks>   
    ///     Usually there is multiple work items for single request. 
    ///     Also work items can be generated dynamically during request processing.
    /// </remarks>
    /// </summary>
    class ResponseHandlerWorkItem : ResponseWorkItem
    {
        /// <summary>
        /// Handler that process given work item
        /// </summary>
        private readonly ResponseHandler _handler;

        private readonly Client _client;

        public ResponseHandlerWorkItem(Client client, ResponseHandler handler)
        {
            _client = client;
            _handler = handler;
        }

        internal override void Run()
        {
            //TODO partial buffer sending
            _handler(_client.Response);
            Complete();
        }

        protected override void onAbort()
        {
            //There is nothing to be aborted
        }
    }
}
