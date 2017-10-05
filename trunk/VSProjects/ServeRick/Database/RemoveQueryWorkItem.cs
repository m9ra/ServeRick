using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    class RemoveQueryWorkItem<ActiveRecord> : DatabaseWorkItem
          where ActiveRecord : DataRecord
    {
        private readonly RemoveQuery<ActiveRecord> _query;

        private readonly Action _action;


        internal RemoveQueryWorkItem(RemoveQuery<ActiveRecord> query, Action action)
        {
            _query = query;
            _action = action;
        }

        internal override void Run()
        {
            var table = Unit.Database.GetTable<ActiveRecord>();
            table.Driver.RemoveRows(table, _query, _handler);
        }

        /// <summary>
        /// Remove execution handler, is called asynchronously, when 
        /// query execution is done.
        /// </summary>
        private void _handler()
        {
            _action?.Invoke();
            Complete();
        }
    }
}
