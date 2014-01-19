using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Diagnostics;

namespace ServeRick.Processing
{
    abstract class WorkProcessor
    {
        readonly Queue<WorkItem> _toProcess = new Queue<WorkItem>();

        readonly object _L_toProcess = new object();

        readonly Thread _executionThread;
        
        internal WorkProcessor()
        {
            _executionThread = new Thread(_run);
            _executionThread.Start();
        }

        internal void EnqueueChain(WorkChain chain)
        {
            var work = chain.ProcessedItem;
            Debug.Assert(work.PlannedProcessor == this, "Cannot process item on current processor");

            lock (_L_toProcess)
            {
                _toProcess.Enqueue(work);
                Monitor.Pulse(_L_toProcess);
            }
        }

        private void _run()
        {
            for (; ; )
            {
                WorkItem work;
                lock (_L_toProcess)
                {
                    if (_toProcess.Count == 0)
                        //there is nothing to process.. wait
                        Monitor.Wait(_L_toProcess);

                    work = _toProcess.Dequeue();
                }

                work.Run();
            }
        }

    }
}
