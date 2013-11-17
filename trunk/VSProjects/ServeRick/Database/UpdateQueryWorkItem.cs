using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    class UpdateQueryWorkItem<ActiveRecord>: DatabaseWorkItem
          where ActiveRecord : DataRecord
    {

        readonly UpdateQuery<ActiveRecord> _query;

        internal UpdateQueryWorkItem(UpdateQuery<ActiveRecord> query)
        {
            _query = query;            
        }

        internal override void Run()
        {
            var table = Unit.Database.GetTable<ActiveRecord>();
            table.Driver.UpdateRows(table, _query, _handler);
        }

        protected override void onComplete()
        {
            //TODO refactor
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
