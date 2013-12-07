using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using ServeRick.Networking;

namespace ServeRick.Processing
{
    /// <summary>
    /// Single item of work, that can be processed in WorkProcessor. It's guaranteed that 
    /// only one item at time accross items belonging to single planned processor.
    /// </summary>
    abstract class WorkItem
    {
        /// <summary>
        /// Work chain where the item belongs to
        /// </summary>
        private WorkChain _owningChain;

        protected ProcessingUnit Unit { get { return _owningChain.Unit; } }

        /// <summary>
        /// Processor where work item will be processed
        /// </summary>
        internal abstract WorkProcessor PlannedProcessor { get; }

        /// <summary>
        /// Determine that item has been completed already
        /// </summary>
        internal bool IsComplete { get; private set; }

        /// <summary>
        /// When overriden run action specified by work item
        /// </summary>
        internal abstract void Run();
        
        internal void Complete()
        {
            if (IsComplete)
                throw new NotSupportedException("Cannot complete work item twice");

            IsComplete = true;

            _owningChain.OnComplete(this);
        }

        internal void Abort()
        {
            throw new NotImplementedException("Work item aborting");
        }

        internal void SetOwningChain(WorkChain workChain)
        {
            if (_owningChain != null)
                throw new InvalidOperationException("Cannot set owning chain multiple times");

            _owningChain = workChain;
        }
    }
}
