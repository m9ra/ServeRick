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
        readonly RemoveQuery<ActiveRecord> _query;

        internal RemoveQueryWorkItem(RemoveQuery<ActiveRecord> query)            
        {
            _query = query;
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
            Complete();
        }
    }
}
