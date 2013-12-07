using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    public delegate void InsertExecutor<ActiveRecord>(IEnumerable<ActiveRecord> insertedRows)
        where ActiveRecord : DataRecord;

    public abstract class InsertQuery
    {
        //prevent creating from other assemblies
        internal InsertQuery()
        {
        }
    }


    public class InsertQuery<ActiveRecord> : InsertQuery
        where ActiveRecord : DataRecord
    {
        public readonly IEnumerable<ActiveRecord> Rows;

        /// <summary>
        /// Insert records specified by given enumeration. 
        /// <remarks>Given enumeration is passed as it is - changing it will lead to unexpected
        /// results</remarks>
        /// </summary>
        /// <param name="records">Recoreds that will be inserted</param>
        internal InsertQuery(IEnumerable<ActiveRecord> records)
        {
            Rows = records;
        }

        internal InsertQueryWorkItem<ActiveRecord> CreateWork(InsertExecutor<ActiveRecord> executor)
        {
            return new InsertQueryWorkItem<ActiveRecord>(this, executor);
        }
    }
}
