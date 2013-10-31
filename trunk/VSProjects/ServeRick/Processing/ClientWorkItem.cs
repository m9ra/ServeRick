using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using ServeRick.Processing;
using ServeRick.Networking;

namespace ServeRick.Processing
{
    /// <summary>
    /// Single item of work, that can be processed in WorkProcessor. It's guaranteed that 
    /// only one item at time accross items belonging to single client is active in whole processing unit.
    /// </summary>
    abstract class ClientWorkItem:WorkItem
    {
        protected Client Client { get; private set; }

        protected ProcessingUnit Unit { get { return Client.Unit; } }

        /// <summary>
        /// Get processor planned for current work item.
        /// </summary>
        /// <returns>Planned processor.</returns>
        abstract protected WorkProcessor getPlannedProcessor();

        /// <summary>
        /// Set client to current work item.
        /// </summary>
        /// <param name="client">Client owning current work item.</param>
        internal void SetClient(Client client)
        {
            if (Client != null)
                throw new NotSupportedException("Cannot change client");

            Debug.Assert(client != null,"Client cannot be null");
            Client = client;

            PlanProcessor(getPlannedProcessor());
        }

        protected override void onComplete()
        {
            Client.ProcessNextWorkItem();
        }
    }
}
