using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    /// <summary>
    /// Execute handler for row retrieved by query.
    /// </summary>
    /// <typeparam name="ActiveRecord">Type of active record</typeparam>
    /// <param name="row"></param>
    public delegate void RowExecutor<ActiveRecord>(ActiveRecord row);

    /// <summary>
    /// Execute handler for rows retrieved by query.
    /// TODO add handler with fetch next row semantic for large record sets.
    /// </summary>
    /// <typeparam name="ActiveRecord">Type of active record</typeparam>
    /// <param name="rows">Retrieved rows</param>
    public delegate void RowsExecutor<ActiveRecord>(ActiveRecord[] rows);

    /// <summary>
    /// Base class for query processed on table
    /// </summary>
    public abstract class TableQuery
    {
        internal TableQuery()
        {
            //prevent creating from another assemblies
        }
    }

    /// <summary>
    /// Query on table representation.
    /// </summary>
    /// <typeparam name="ActiveRecord">Tyep of active record.</typeparam>
    public class TableQuery<ActiveRecord> : TableQuery
        where ActiveRecord : DataRecord
    {
        public readonly WhereClause Condition;

        public readonly int MaxCount;

        private readonly Response _response;

        internal TableQuery(Response response)
            : this(response, new WhereClause(),int.MaxValue)
        { }

        internal TableQuery(Response response, WhereClause where, int limit)
        {
            _response = response;
            Condition = where;
            MaxCount = limit;
        }

        public TableQuery<ActiveRecord> Where(string column, object operand)
        {
            var item = new WhereItem(column, WhereOperation.Equal, operand);
            var conditon = Condition.Clone();

            conditon.Add(item);
            return new TableQuery<ActiveRecord>(_response, conditon,MaxCount);
        }

        public TableQuery<ActiveRecord> Limit(int maxCount)
        {
            return new TableQuery<ActiveRecord>(_response, Condition, maxCount);
        }

        public void ExecuteRow(RowExecutor<ActiveRecord> executor)
        {
            var work = new RowQueryWorkItem<ActiveRecord>(this, executor);
            _response.Client.EnqueueWork(work);
        }

        public void ExecuteRows(RowsExecutor<ActiveRecord> executor)
        {
            var work = new RowsQueryWorkItem<ActiveRecord>(this, executor);
            _response.Client.EnqueueWork(work);
        }
    }
}
