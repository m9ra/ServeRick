using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    class RowQueryWorkItem<ActiveRecord>: DatabaseWorkItem
        where ActiveRecord : DataRecord
    {
        /// <summary>
        /// Query stored for work item.
        /// </summary>
        private readonly SelectQuery<ActiveRecord> _query;

        /// <summary>
        /// Executor processed with retrieved row.
        /// </summary>
        private readonly RowExecutor<ActiveRecord> _executor;

        internal RowQueryWorkItem(SelectQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
        {
            _query = query;
            _executor = executor;
        }


        internal override void Run()
        {
            var table = Unit.Database.GetTable<ActiveRecord>();
            table.Driver.ExecuteRow(table, _query, _handler);
        }

        /// <summary>
        /// Row execution handler, is called asynchronously, when 
        /// query execution is done.
        /// </summary>
        /// <param name="row">Retrieved row.</param>
        private void _handler(ActiveRecord row) {
            _executor(row);
            Complete();
        }
    }
}
