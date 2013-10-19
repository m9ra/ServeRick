using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

namespace SharpServer.Networking
{
    class InputProcessor
    {
        readonly Queue<InputWorkItem> _toProcess = new Queue<InputWorkItem>();

        readonly object _L_toProcess = new object();

        readonly Thread _executionThread;


        internal InputProcessor()
        {
            _executionThread = new Thread(_run);
            _executionThread.Start();
        }

        internal void EnqueueWork(InputWorkItem work)
        {
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
                InputWorkItem work;
                lock (_L_toProcess)
                {
                    if (_toProcess.Count == 0)
                        //there is nothing to process.. wait
                        Monitor.Wait(_L_toProcess);

                    work = _toProcess.Dequeue();
                }

                var input = work.Client.Input;
                throw new NotImplementedException();
            }
        }
    }

}
