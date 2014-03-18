using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    /// <summary>
    /// Driver for fetching active record data. Doesn't need to be thread safe.
    /// Query methods are called synchronously - they cannot block!!
    /// </summary>
    public abstract class DataDriver
    {
        public abstract void ExecuteRow<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
            where ActiveRecord : DataRecord;

        public abstract void ExecuteRows<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowsExecutor<ActiveRecord> executor)
            where ActiveRecord : DataRecord;

        public abstract void InsertRows<ActiveRecord>(DataTable<ActiveRecord> table, InsertQuery<ActiveRecord> query, InsertExecutor<ActiveRecord> executor)
            where ActiveRecord : DataRecord;

        public abstract void UpdateRows<ActiveRecord>(DataTable<ActiveRecord> table, UpdateQuery<ActiveRecord> query, Action executor)
            where ActiveRecord : DataRecord;

        public abstract void RemoveRows<ActiveRecord>(DataTable<ActiveRecord> table, RemoveQuery<ActiveRecord> query, Action executor)
            where ActiveRecord : DataRecord;

        public abstract void Call<ActiveRecord>(DataTable<ActiveRecord> table, CallQuery<ActiveRecord> query, Action executor)
            where ActiveRecord : DataRecord;

        public abstract void Initialize<ActiveRecord>(DataTable<ActiveRecord> table)
            where ActiveRecord : DataRecord;
    }
}
