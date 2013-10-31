using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    public delegate void RowExecutor<ActiveRecord>(ActiveRecord retrievedRow);

    public abstract class TableQuery
    {
    }

    public class TableQuery<ActiveRecord> : TableQuery
        where ActiveRecord : DataRecord
    {
        public readonly WhereClause Condition;

        private readonly Response _response;

        internal TableQuery(Response response)
            : this(response, new WhereClause())
        { }

        internal TableQuery(Response response, WhereClause where)
        {
            _response = response;
            Condition = where;
        }

        public TableQuery<ActiveRecord> Where(string column, object operand)
        {
            var item = new WhereItem(column, WhereOperation.Equal, operand);
            var conditon = Condition.Clone();

            conditon.Add(item);
            return new TableQuery<ActiveRecord>(_response, conditon);
        }

        public void ExecuteRow(RowExecutor<ActiveRecord> executor)
        {
            var work = new RowQueryWorkItem<ActiveRecord>(this, executor);
            _response.Client.EnqueueWork(work);
        }
    }
}
