using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using SharpServer.Networking;

namespace SharpServer.Responsing
{
    class ResponseProcessor
    {
        readonly Queue<ResponseWork> _toProcess = new Queue<ResponseWork>();

        readonly object _L_toProcess = new object();

        //TODO only for testing purposes
        readonly ResponseHandler _outOfServicePage;
        readonly Thread _executionThread;


        internal ResponseProcessor(ResponseHandler outOfServicePage)
        {
            _outOfServicePage = outOfServicePage;
            _executionThread = new Thread(_run);
            _executionThread.Start();
        }

        internal void MakeResponse(Client client)
        {
            var work = new ResponseWork(client, _outOfServicePage);
            client.Response = new Response(client,this);

            EnqueueWork(work);
        }

        internal void EnqueueWork(ResponseWork work)
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
                ResponseWork work;
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
