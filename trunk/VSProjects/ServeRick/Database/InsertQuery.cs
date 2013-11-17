using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// TODO response is not needed - refactor it
        /// </summary>
        private readonly Response _response;

        /// <summary>
        /// Insert records specified by given enumeration. 
        /// <remarks>Given enumeration is passed as it is - changing it will lead to unexpected
        /// results</remarks>
        /// </summary>
        /// <param name="records">Recoreds that will be inserted</param>
        internal InsertQuery(Response response, IEnumerable<ActiveRecord> records)
        {
            _response = response;
            Rows = records;
        }

        public void Execute(InsertExecutor<ActiveRecord> executor)
        {
            var work = new InsertQueryWorkItem<ActiveRecord>(this, executor);
            _response.Client.EnqueueWork(work);
        }
    }
}
