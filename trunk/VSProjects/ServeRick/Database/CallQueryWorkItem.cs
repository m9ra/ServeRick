using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    class CallQueryWorkItem<ActiveRecord> : DatabaseWorkItem
          where ActiveRecord : DataRecord
    {

        readonly CallQuery<ActiveRecord> _query;

        internal CallQueryWorkItem(CallQuery<ActiveRecord> query)
            : base()
        {
            _query = query;
        }

        internal override void Run()
        {
            var table = Unit.Database.GetTable<ActiveRecord>();
            table.Driver.Call(table, _query, _handler);
        }

        /// <summary>
        /// Update execution handler, is called asynchronously, when 
        /// query execution is done.
        /// </summary>
        private void _handler()
        {
            Complete();
        }
    }
}
