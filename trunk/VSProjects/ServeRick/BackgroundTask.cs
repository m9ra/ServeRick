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
        /// Lock for making db calls blocking.
        /// </summary>
        private readonly object _L_databaseSync = new object();

        /// <summary>
        /// Used for holding server from running before task is initalized.
        /// </summary>
        private readonly ManualResetEvent _initializationLock = new ManualResetEvent(false);

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

        protected virtual void initalize()
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
        /// Waits until task is initialized.
        /// </summary>
        internal void WaitUntilInitialized()
        {
            _initializationLock.WaitOne();
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
                initalize();
                _initializationLock.Set(); //initialization is complete

                run();
            }
            finally
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// Pause current task for given time
        /// </summary>
        /// <param name="time">Time in millisecond</param>
        protected void Pause(int time)
        {
            Thread.Sleep(time);
        }

        #region Database API

        protected static SelectQuery<ActiveRecord> Query<ActiveRecord>()
         where ActiveRecord : DataRecord
        {
            return new SelectQuery<ActiveRecord>();
        }

        protected static InsertQuery<ActiveRecord> Insert<ActiveRecord>(IEnumerable<ActiveRecord> entries)
         where ActiveRecord : DataRecord
        {
            return new InsertQuery<ActiveRecord>(entries);
        }

        protected static CallQuery<ActiveRecord> Call<ActiveRecord>(string callName, IEnumerable<KeyValuePair<string, object>> arguments = null)
            where ActiveRecord : DataRecord
        {
            return new CallQuery<ActiveRecord>(callName, arguments);
        }

        protected void Execute<T>(CallQuery<T> query, Action action = null)
         where T : DataRecord
        {
            var item = query.CreateWork(action);
            enqueue(item);
        }

        protected void BlockingExecute<T>(CallQuery<T> query)
      where T : DataRecord
        {
            requireDbLockAccess();

            lock (_L_databaseSync)
            {
                Execute(query, unlockDbAction);
                //wait until query is finished
                Monitor.Wait(_L_databaseSync);
            }
        }

        protected void Execute<T>(UpdateQuery<T> query, UpdateExecutor<T> executor = null)
         where T : DataRecord
        {
            var item = query.CreateWork(executor);
            enqueue(item);
        }

        protected void BlockingExecute<T>(UpdateQuery<T> query)
where T : DataRecord
        {
            requireDbLockAccess();

            lock (_L_databaseSync)
            {
                Execute(query, _ => unlockDbAction());
                //wait until query is finished
                Monitor.Wait(_L_databaseSync);
            }
        }

        protected void Execute<T>(RemoveQuery<T> query, Action action)
         where T : DataRecord
        {
            var item = query.CreateWork(action);
            enqueue(item);
        }

        protected void BlockingExecute<T>(RemoveQuery<T> query)
 where T : DataRecord
        {
            requireDbLockAccess();

            lock (_L_databaseSync)
            {
                Execute(query, unlockDbAction);
                //wait until query is finished
                Monitor.Wait(_L_databaseSync);
            }
        }

        protected void Execute<T>(InsertQuery<T> query, InsertExecutor<T> executor = null)
         where T : DataRecord
        {
            var item = query.CreateWork(executor);
            enqueue(item);
        }


        protected void BlockingExecute<T>(InsertQuery<T> query)
where T : DataRecord
        {
            requireDbLockAccess();

            lock (_L_databaseSync)
            {
                Execute(query, _ => unlockDbAction());
                //wait until query is finished
                Monitor.Wait(_L_databaseSync);
            }
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

        protected T[] BlockingExecuteRows<T>(SelectQuery<T> query)
    where T : DataRecord
        {
            requireDbLockAccess();

            lock (_L_databaseSync)
            {
                T[] resultRows = null;
                ExecuteRows(query, result =>
                {
                    resultRows = result.Rows;
                    unlockDbAction();
                });
                //wait until query is finished
                Monitor.Wait(_L_databaseSync);

                return resultRows;
            }
        }

        protected void ExecuteForeach<ItemType, ActiveRecord>(IEnumerable<ItemType> enumeration, Func<ItemType, CallQuery<ActiveRecord>> callback, Action afterAction = null)
            where ActiveRecord : DataRecord
        {
            var toEnumerate = new Queue<ItemType>(enumeration);

            enumerationHandler(toEnumerate, callback, afterAction);
        }

        #endregion

        private void enumerationHandler<ItemType, ActiveRecord>(Queue<ItemType> toEnumerate, Func<ItemType, CallQuery<ActiveRecord>> callback, Action afterAction = null)
            where ActiveRecord : DataRecord
        {
            if (toEnumerate.Count == 0)
            {
                //everything is enumerated now
                afterAction();
                return;
            }

            var current = toEnumerate.Dequeue();
            var query = callback(current);

            Execute(query, () =>
            {
                enumerationHandler(toEnumerate, callback, afterAction);
            });
        }

        private void enqueue(WorkItem item)
        {
            _unit.EnqueueIndependent(item);
        }

        private void unlockDbAction()
        {
            lock (_L_databaseSync)
                Monitor.Pulse(_L_databaseSync);
        }

        private void requireDbLockAccess()
        {
            if (Thread.CurrentThread != _thread)
                throw new InvalidOperationException("Only task thread can lock Db.");
        }
    }
}
