using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;


using ServeRick.Database;

namespace ServeRick.Modules.MySQL
{
    public class MySqlDriver : DataDriver
    {
        /// <summary>
        /// Work queue where jobs are stored
        /// </summary>
        private readonly Queue<QueryWork> _workQueue = new Queue<QueryWork>();

        private readonly int _connectionPingTime = 2 * 60 * 1000;

        /// <summary>
        /// Lock for queue
        /// </summary>
        private readonly object _L_Queue = new object();

        /// <summary>
        /// Wokers available for driver
        /// </summary>
        private readonly List<QueryWorker> _workers = new List<QueryWorker>();

        /// <summary>
        /// Connection string used by workers to connect database
        /// </summary>
        internal readonly string ConnectionString;

        /// <summary>
        /// Create MySqlDriver which will connect to database according to
        /// given connection
        /// </summary>
        /// <param name="connectionString">Connection string for connecting to database</param>
        public MySqlDriver(string connectionString, int workersCount = 5)
        {
            ConnectionString = connectionString;

            for (int i = 0; i < workersCount; ++i)
            {
                _workers.Add(new QueryWorker(this));
            }
        }

        /// <summary>
        /// Enqueue job that will be processed on workers pool
        /// </summary>
        /// <param name="work">Enqueue work</param>
        private void enqueueWork(QueryWork work)
        {
            lock (_L_Queue)
            {
                _workQueue.Enqueue(work);
                Monitor.Pulse(_L_Queue);
            }
        }

        /// <summary>
        /// Blockingly dequeue work item. Is used by 
        /// query workers from separate threads
        /// </summary>
        /// <returns>Dequeued work</returns>
        internal QueryWork DequeueWork()
        {
            lock (_L_Queue)
            {
                if (_workQueue.Count == 0)
                {
                    Monitor.Wait(_L_Queue, _connectionPingTime);
                    if (_workQueue.Count == 0)
                        return new QueryWork(_ping);
                }

                return _workQueue.Dequeue();
            }
        }

        private void _ping(QueryWorker worker) {
            worker.Ping();
        }

        #region Driver implementation wrapped on the pool

        public override void ExecuteRow<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
        {
            enqueueWork((w) =>
            {
                w.ExecuteRow(table, query, executor);
            });
        }

        public override void ExecuteRows<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowsExecutor<ActiveRecord> executor)
        {
            enqueueWork((w) =>
            {
                w.ExecuteRows(table, query, executor);
            });
        }

        public override void InsertRows<ActiveRecord>(DataTable<ActiveRecord> table, InsertQuery<ActiveRecord> query, InsertExecutor<ActiveRecord> executor)
        {
            enqueueWork((w) =>
            {
                w.InsertRows(table, query, executor);
            });
        }

        public override void UpdateRows<ActiveRecord>(DataTable<ActiveRecord> table, UpdateQuery<ActiveRecord> query, Action executor)
        {
            enqueueWork((w) =>
            {
                w.UpdateRows(table, query, executor);
            });
        }

        public override void RemoveRows<ActiveRecord>(DataTable<ActiveRecord> table, RemoveQuery<ActiveRecord> query, Action executor)
        {
            enqueueWork((w) =>
            {
                w.RemoveRows(table, query, executor);
            });
        }

        public override void Initialize<ActiveRecord>(DataTable<ActiveRecord> table)
        {
            enqueueWork((w) =>
            {
                w.Initialize(table);
            });
        }

        #endregion
    }
}
