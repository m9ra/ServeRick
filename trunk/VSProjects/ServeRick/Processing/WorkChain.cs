using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace ServeRick.Processing
{
    class WorkChain
    {
        /// <summary>
        /// Stored work items
        /// </summary>
        private readonly LinkedList<WorkItem> _items = new LinkedList<WorkItem>();

        /// <summary>
        /// Work item used as cursor for inserting next work items.
        /// Inserted work item is appended behind cursor item
        /// </summary>
        private LinkedListNode<WorkItem> _cursorItem;

        /// <summary>
        /// Work item that is currently processed
        /// </summary>
        private LinkedListNode<WorkItem> _processedItem;

        /// <summary>
        /// Determine that abort operation has been called on current workchain
        /// </summary>
        private volatile bool _isAborted = false;

        /// <summary>
        /// Handler called 
        /// </summary>
        internal event Action OnCompleted;

        /// <summary>
        /// Unit where work chain can be processed
        /// </summary>
        internal readonly ProcessingUnit Unit;

        /// <summary>
        /// Determine that chain has been completed
        /// </summary>
        internal bool IsComplete { get; private set; }

        /// <summary>
        /// Item that is currently processed by its planned processor
        /// </summary>
        internal WorkItem ProcessedItem { get { return _processedItem.Value; } }

        internal WorkChain(ProcessingUnit unit)
        {
            Unit = unit;
        }

        /// <summary>
        /// Insert item behind cursor and became cursor position
        /// </summary>
        /// <param name="item">Inserted work item</param>
        internal void InsertItem(WorkItem item)
        {
            initializeItem(item);
            if (_cursorItem == null)
            {
                _cursorItem = _items.AddLast(item);
            }
            else
            {
                _cursorItem = _items.AddAfter(_cursorItem, item);
            }
        }

        internal void StartProcessing()
        {
            if (_processedItem != null || IsComplete)
            {
                throw new InvalidOperationException("Cannot start work chain processing in current state");
            }

            _processedItem = _items.First;
            enqueueToProcessor();
        }

        /// <summary>
        /// Append work item at the end of chain
        /// </summary>
        /// <param name="item">Appended work item</param>
        internal void AppendItem(WorkItem item)
        {
            //TODO think about semantics - should appending affect cursor ?
            initializeItem(item);
            _items.AddLast(item);
        }

        /// <summary>
        /// Is called by work items when completed
        /// </summary>
        /// <param name="item">Completed work item</param>
        internal void OnComplete(WorkItem item)
        {
            if (item != _processedItem.Value)
                throw new InvalidOperationException("Cannot complete given item in current state");

            //shift to next item
            _processedItem = _processedItem.Next;

            if (_processedItem == null || _isAborted)
            {
                //there is no other item                
                IsComplete = true;
                if (OnCompleted != null)
                    OnCompleted();
            }
            else
            {
                enqueueToProcessor();
            }
        }

        private void enqueueToProcessor()
        {
            ProcessedItem.PlannedProcessor.EnqueueChain(this);
        }

        private void initializeItem(WorkItem item)
        {
            item.SetOwningChain(this);
        }

        internal void Abort()
        {
            if (!IsComplete)
            {
                _isAborted = true;

                var toAbort = _cursorItem;
                while (toAbort != null)
                {
                    toAbort.Value.Abort();
                    toAbort = toAbort.Next;
                }

          /*      if (OnCompleted != null)
                    //run completition routines if needed
                    OnCompleted();*/
            }
        }
    }
}
