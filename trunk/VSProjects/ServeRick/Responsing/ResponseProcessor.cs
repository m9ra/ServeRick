using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using ServeRick.Networking;

namespace ServeRick.Responsing
{
    class ResponseProcessor
    {
        readonly Queue<ResponseWorkItem> _toProcess = new Queue<ResponseWorkItem>();

        readonly object _L_toProcess = new object();

        readonly Thread _executionThread;


        internal ResponseProcessor()
        {
            _executionThread = new Thread(_run);
            _executionThread.Start();
        }

        internal void EnqueueWork(ResponseWorkItem work)
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
                ResponseWorkItem work;
                lock (_L_toProcess)
                {
                    if (_toProcess.Count == 0)
                        //there is nothing to process.. wait
                        Monitor.Wait(_L_toProcess);

                    work= _toProcess.Dequeue();
                }

                var response = work.Client.Response;
                response.RunWork(work);
            }
        }
    }
}
