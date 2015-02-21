using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    public delegate void UpdateExecutor<ActiveRecord>(int updatedRows)
        where ActiveRecord : DataRecord;

    public abstract class UpdateQuery
    {
        //prevent creating from other assemblies
        internal UpdateQuery()
        {
        }
    }

    public class UpdateQuery<ActiveRecord> : UpdateQuery
        where ActiveRecord : DataRecord
    {
        public readonly SelectQuery<ActiveRecord> Select;

        public readonly IEnumerable<KeyValuePair<string, object>> Updates;

        internal UpdateQuery(SelectQuery<ActiveRecord> select)
        {
            Select = select;
            Updates = new KeyValuePair<string, object>[0];
        }

        private UpdateQuery(SelectQuery<ActiveRecord> select, IEnumerable<KeyValuePair<string, object>> updates)
        {
            Select = select;
            Updates = updates;
        }

        public UpdateQuery<ActiveRecord> Update(string column, object value)
        {
            var updates = new List<KeyValuePair<string, object>>(Updates);
            updates.Add(new KeyValuePair<string, object>(column, value));
            return new UpdateQuery<ActiveRecord>(Select, updates);
        }

        /// <summary>
        /// Execute without queueing for client
        /// </summary>
        internal UpdateQueryWorkItem<ActiveRecord> CreateWork(UpdateExecutor<ActiveRecord> executor)
        {
            return new UpdateQueryWorkItem<ActiveRecord>(this,executor);
        }
    }
}
