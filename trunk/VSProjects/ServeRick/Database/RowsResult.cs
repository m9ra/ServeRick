using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    public class RowsResult<ActiveRecord>
        where ActiveRecord : DataRecord
    {
        /// <summary>
        /// Total count of items as if there was no limit
        /// </summary>
        public readonly int TotalCount;

        /// <summary>
        /// Retrieved rows
        /// </summary>
        public readonly ActiveRecord[] Rows;

        public RowsResult(IEnumerable<ActiveRecord> rows, int totalCount)
        {
            Rows = rows.ToArray();
            TotalCount = totalCount;
        }
    }
}
