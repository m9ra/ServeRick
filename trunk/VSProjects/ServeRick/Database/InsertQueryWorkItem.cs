using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    class InsertQueryWorkItem<ActiveRecord> : DatabaseWorkItem
          where ActiveRecord : DataRecord
    {

        readonly InsertQuery<ActiveRecord> _query;
        readonly InsertExecutor<ActiveRecord> _executor;

        internal InsertQueryWorkItem(ProcessingUnit unit, InsertQuery<ActiveRecord> query, InsertExecutor<ActiveRecord> executor)
            :base(unit)
        {
            _query = query;
            _executor = executor;
        }

        internal override void Run()
        {
            var table = Unit.Database.GetTable<ActiveRecord>();
            table.Driver.InsertRows(table, _query, _handler);
        }

        /// <summary>
        /// Insert execution handler, is called asynchronously, when 
        /// query execution is done.
        /// </summary>
        /// <param name="rows">Inserted rows.</param>
        private void _handler(IEnumerable<ActiveRecord> rows)
        {
            _executor(rows);
            Complete();
        }
    }
}
