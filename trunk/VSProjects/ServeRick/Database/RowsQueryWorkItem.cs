using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    class RowsQueryWorkItem<ActiveRecord> : DatabaseWorkItem
          where ActiveRecord : DataRecord
    {
        /// <summary>
        /// Query stored for work item.
        /// </summary>
        private readonly SelectQuery<ActiveRecord> _query;

        /// <summary>
        /// Executor processed with retrieved row.
        /// </summary>
        private readonly RowsExecutor<ActiveRecord> _executor;

        internal RowsQueryWorkItem(SelectQuery<ActiveRecord> query, RowsExecutor<ActiveRecord> executor)
        {
            _query = query;
            _executor = executor;
        }


        internal override void Run()
        {
            var table = Unit.Database.GetTable<ActiveRecord>();
            table.Driver.ExecuteRows(table, _query, _handler);
        }

        /// <summary>
        /// Row execution handler, is called asynchronously, when 
        /// query execution is done.
        /// </summary>
        /// <param name="rows">Retrieved rows.</param>
        private void _handler(RowsResult<ActiveRecord> rows)
        {
            _executor(rows);
            Complete();
        }
    }
}
