using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    class DatabaseProcessor : WorkProcessor
    {
        Dictionary<Type, DataTable> _tables = new Dictionary<Type, DataTable>();

        internal void AddTable(DataTable table)
        {
            var type = table.GetRecordType();
            _tables[type] = table;
        }

        internal DataDriver<ActiveRecord> GetDriver<ActiveRecord>()
            where ActiveRecord : DataRecord
        {
            var type = typeof(ActiveRecord);
            var table = _tables[type] as DataTable<ActiveRecord>;

            return table.Driver;
        }
    }
}
