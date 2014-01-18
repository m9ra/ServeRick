using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using ServeRick.Database;
using ServeRick.Processing;

namespace ServeRick
{
    /// <summary>
    /// Class representing task that is runned in background of server.
    /// It has access to services of corresponding Processing Unit
    /// </summary>
    public abstract class BackgroundTask
    {
        /// <summary>
        /// Thread where task is running
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// Processing unit where
        /// </summary>
        private ProcessingUnit _unit;

        /// <summary>
        /// Determine that task has been stopped;
        /// </summary>
        private volatile bool _isStopped;

        /// <summary>
        /// Server where task is running
        /// </summary>
        protected HttpServer Server { get; private set; }

        /// <summary>
        /// Determine that task is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Determine that task has been stopped
        /// <remarks>Has volatile behaviour, so it can be used accross threads safely</remarks>
        /// </summary>
        public bool IsStopped { get { return _isStopped; } }

        /// <summary>
        /// Method that is executed in separated thread. Thread is exclusively
        /// used only for running this method.
        /// <remarks>Processin unit services are available only when run is called</remarks>
        /// </summary>
        protected abstract void run();

        /// <summary>
        /// Handler called when task is stopped
        /// </summary>
        protected virtual void onStopped()
        {
            //there is nothing to do by default
        }

        /// <summary>
        /// Run background task in separate thread.
        /// </summary>
        /// <param name="unit">Processing unit that current taks corresponds to</param>
        internal void Run(HttpServer server)
        {
            if (IsStopped)
                throw new InvalidOperationException("Cannot run task when its stopped");

            if (_thread != null)
                throw new NotSupportedException("Cannot run task twice");

            Server = server;
            _unit = Server.Unit;
            _thread = new Thread(_run);
            _thread.Start();
        }

        /// <summary>
        /// Stop task run
        /// </summary>
        public void Stop()
        {
            if (IsStopped)
                throw new InvalidOperationException("Cannot stop twice");

            _isStopped = true;
            onStopped();
            _thread.Interrupt();
        }

        /// <summary>
        /// Callback that is runned in thread
        /// </summary>
        private void _run()
        {
            IsRunning = true;

            try
            {
                run();
            }
            finally
            {
                IsRunning = false;
            }
        }


        #region Database API

        protected static SelectQuery<ActiveRecord> Query<ActiveRecord>()
         where ActiveRecord : DataRecord
        {
            return new SelectQuery<ActiveRecord>();
        }

        protected InsertQuery<ActiveRecord> Insert<ActiveRecord>(IEnumerable<ActiveRecord> entries)
            where ActiveRecord : DataRecord
        {
            return new InsertQuery<ActiveRecord>(entries);
        }

        protected void Execute<T>(UpdateQuery<T> query)
         where T : DataRecord
        {
            var item = query.CreateWork();
            enqueue(item);
        }

        protected void Execute<T>(RemoveQuery<T> query)
         where T : DataRecord
        {
            var item = query.CreateWork();
            enqueue(item);
        }

        protected void Execute<T>(InsertQuery<T> query, InsertExecutor<T> executor)
            where T : DataRecord
        {
            var item = query.CreateWork(executor);
            enqueue(item);
        }

        protected void ExecuteRow<T>(SelectQuery<T> query, RowExecutor<T> executor)
            where T : DataRecord
        {
            var item = query.CreateWork(executor);
            enqueue(item);
        }

        protected void ExecuteRows<T>(SelectQuery<T> query, RowsExecutor<T> executor)
            where T : DataRecord
        {
            var item = query.CreateWork(executor);
            enqueue(item);
        }

        #endregion

        private void enqueue(WorkItem item)
        {
            _unit.EnqueueIndependent(item);
        }
    }
}
