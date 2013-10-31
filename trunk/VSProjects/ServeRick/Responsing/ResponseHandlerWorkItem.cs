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
    class ResponseHandlerWorkItem : ClientWorkItem
    {
        /// <summary>
        /// Handler that process given work item
        /// </summary>
        internal readonly ResponseHandler Handler;

        public ResponseHandlerWorkItem(ResponseHandler handler)
        {
            Handler = handler;
        }

        internal override void Run()
        {
            //TODO partial buffer sending
            Handler(Client.Response);
            Complete();
        }

        protected override WorkProcessor getPlannedProcessor()
        {
            return Unit.Output;
        }
    }
}
