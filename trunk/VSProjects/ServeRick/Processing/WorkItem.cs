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
        /// Determine that work item has already been enqueued at processor
        /// </summary>
        private bool _wasEnqueued = false;

        /// <summary>
        /// Action invoked when item is completed
        /// </summary>
        internal event Action OnComplete;

        /// <summary>
        /// Processor where work item will be processed
        /// </summary>
        internal WorkProcessor PlannedProcessor { get; private set; }

        /// <summary>
        /// Determine that item has been completed already
        /// </summary>
        internal bool IsComplete { get; private set; }

        /// <summary>
        /// When overriden run action specified by work item
        /// </summary>
        internal abstract void Run();

        /// <summary>
        /// Plan processor for current work item. Planned
        /// processor will be used for runnig current work item.
        /// </summary>
        /// <param name="plannedProcessor">Processor planned for this item.</param>
        protected void PlanProcessor(WorkProcessor plannedProcessor)
        {
            if (PlannedProcessor != null)
                throw new NotSupportedException("Cannot replan processor for work item");

            Debug.Assert(plannedProcessor != null, "Planned processor cannot be null");
            PlannedProcessor = plannedProcessor;
        }

        internal void Complete()
        {
            if (IsComplete)
                throw new NotSupportedException("Cannot complete work item twice");

            IsComplete = true;
            onComplete();
            if (OnComplete != null)
                OnComplete();
        }

        /// <summary>
        /// Method is called whenever work item is completed
        /// </summary>
        protected virtual void onComplete()
        {
            //nothing to do by default
        }

        /// <summary>
        /// Enqueue current work item on planned processor
        /// </summary>
        internal void EnqueueToProcessor()
        {
            Debug.Assert(_wasEnqueued == false, "Work item can be processed only once");
            _wasEnqueued = true;
            PlannedProcessor.EnqueueWork(this);
        }
    }
}
