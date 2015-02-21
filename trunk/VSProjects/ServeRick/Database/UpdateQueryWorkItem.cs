using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    class UpdateQueryWorkItem<ActiveRecord> : DatabaseWorkItem
          where ActiveRecord : DataRecord
    {

        readonly UpdateQuery<ActiveRecord> _query;

        readonly UpdateExecutor<ActiveRecord> _executor;

        internal UpdateQueryWorkItem(UpdateQuery<ActiveRecord> query,UpdateExecutor<ActiveRecord> executor)
            : base()
        {
            _query = query;
            _executor = executor;
        }

        internal override void Run()
        {
            var table = Unit.Database.GetTable<ActiveRecord>();
            table.Driver.UpdateRows(table, _query, _handler);
        }

        /// <summary>
        /// Update execution handler, is called asynchronously, when 
        /// query execution is done.
        /// </summary>
        private void _handler(int updatedRows)
        {
            if (_executor != null)
                _executor(updatedRows);
            Complete();
        }
    }
}
