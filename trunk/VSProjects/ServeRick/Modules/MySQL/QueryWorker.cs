using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using MySql.Data;
using MySql.Data.MySqlClient;

using ServeRick.Database;

namespace ServeRick.Modules.MySQL
{
    /// <summary>
    /// Definition of work on query workers
    /// </summary>
    /// <param name="worker">Worker available </param>
    delegate void QueryWork(QueryWorker worker);

    class QueryWorker : DataDriver
    {
        /// <summary>
        /// Connection used by worker
        /// </summary>
        private MySqlConnection _connection;

        /// <summary>
        /// Owning driver which work is handled by current worker
        /// </summary>
        private readonly MySqlDriver _owner;

        /// <summary>
        /// Thread used by current worker
        /// </summary>
        private readonly Thread _thread;

        internal QueryWorker(MySqlDriver driver)
        {
            _owner = driver;
            _thread = new Thread(_run);
            _thread.Start();
        }

        /// <summary>
        /// Run in separated thread and handle owners work
        /// </summary>
        private void _run()
        {
            prepareWorker();

            for (; ; )
            {
                var work = _owner.DequeueWork();
                if (work == null)
                    break;

                work(this);
            }
        }

        /// <summary>
        /// Prepare connection used by worker
        /// </summary>
        private void prepareWorker()
        {
            _connection = new MySqlConnection(_owner.ConnectionString);
            _connection.Open();
        }

        #region Driver processing methods

        public override void ExecuteRow<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
        {
            throw new NotImplementedException();
        }

        public override void ExecuteRows<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowsExecutor<ActiveRecord> executor)
        {
            throw new NotImplementedException();
        }

        public override void InsertRows<ActiveRecord>(DataTable<ActiveRecord> table, InsertQuery<ActiveRecord> query, InsertExecutor<ActiveRecord> executor)
        {
            throw new NotImplementedException();
        }

        public override void UpdateRows<ActiveRecord>(DataTable<ActiveRecord> table, UpdateQuery<ActiveRecord> query, Action executor)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRows<ActiveRecord>(DataTable<ActiveRecord> table, RemoveQuery<ActiveRecord> query, Action executor)
        {
            throw new NotImplementedException();
        }

        public override void Initialize<ActiveRecord>(DataTable<ActiveRecord> table)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
