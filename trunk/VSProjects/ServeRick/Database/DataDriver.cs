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
    /// <typeparam name="ActiveRecord">Type of drived ActiveRecord</typeparam>
    public abstract class DataDriver<ActiveRecord>
        where ActiveRecord:DataRecord
    {
        public abstract void ExecuteRow(TableQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor);
    }
}
