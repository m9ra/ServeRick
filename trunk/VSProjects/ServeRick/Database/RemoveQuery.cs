using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    public abstract class RemoveQuery
    {
        //prevent creating from other assemblies
        internal RemoveQuery()
        {
        }
    }

    public class RemoveQuery<ActiveRecord> : RemoveQuery
        where ActiveRecord : DataRecord
    {
        public readonly SelectQuery<ActiveRecord> Select;


        internal RemoveQuery(SelectQuery<ActiveRecord> select)
        {
            Select = select;
        }

        /// <summary>
        /// Execute without queueing for client
        /// </summary>
        internal RemoveQueryWorkItem<ActiveRecord> CreateWork()
        {
            return new RemoveQueryWorkItem<ActiveRecord>(this);
        }
    }

}
