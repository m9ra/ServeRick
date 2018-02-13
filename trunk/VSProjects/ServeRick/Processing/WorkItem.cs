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

        /// <summary>
        /// Determine that item is aborted
        /// </summary>
        private volatile bool _isAborted;

        protected ProcessingUnit Unit { get { return _owningChain.Unit; } }

        /// <summary>
        /// Handler called whenever work item is aborted
        /// Is usefull for cleanup operations like closing files
        /// </summary>
        protected abstract void onAbort();

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
            {
                Log.Error("Cannot complete work item twice");
                return; 
            }

            IsComplete = true;

            _owningChain.OnComplete(this);
        }

        internal void Abort()
        {
            if (_isAborted)
                return;

            _isAborted = true;
            onAbort();
        }

        internal void SetOwningChain(WorkChain workChain)
        {
            if (_owningChain != null)
                throw new InvalidOperationException("Cannot set owning chain multiple times");

            _owningChain = workChain;
        }
    }
}
