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
    public delegate void RowExecutor<ActiveRecord>(ActiveRecord row)
        where ActiveRecord : DataRecord;

    /// <summary>
    /// Execute handler for rows retrieved by query.
    /// TODO add handler with fetch next row semantic for large record sets.
    /// </summary>
    /// <typeparam name="ActiveRecord">Type of active record</typeparam>
    /// <param name="result">Retrieved rows</param>
    public delegate void RowsExecutor<ActiveRecord>(RowsResult<ActiveRecord> result)
        where ActiveRecord : DataRecord;

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
        private readonly Response _response;

        public readonly WhereClause Condition;

        public int MaxCount { get; private set; }

        public int Start { get; private set; }

        internal TableQuery(Response response)
            : this(response, new WhereClause())
        {
            MaxCount = int.MaxValue;
        }

        internal TableQuery(Response response, WhereClause where)
        {
            _response = response;
            Condition = where;
        }

        public TableQuery<ActiveRecord> Find(int id)
        {
            return Where("id", WhereOperation.Equal, id).Limit(1);
        }

        public TableQuery<ActiveRecord> WhereEqual(string column, object operand)
        {
            return Where(column, WhereOperation.Equal, operand);
        }

        public TableQuery<ActiveRecord> Where(string column, WhereOperation operation, object operand)
        {
            var item = new WhereItem(column, operation, operand);
            var conditon = Condition.Clone();

            conditon.Add(item);
            var query = Clone(conditon);
            return query;
        }

        public TableQuery<ActiveRecord> Limit(int maxCount)
        {
            var clonned = Clone();
            clonned.MaxCount = maxCount;
            return clonned;
        }

        public TableQuery<ActiveRecord> Offset(int start)
        {
            var clonned = Clone();
            clonned.Start = start;
            return clonned;
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

        internal TableQuery<ActiveRecord> Clone(WhereClause where)
        {
            var query = new TableQuery<ActiveRecord>(_response, where);
            query.Start = Start;
            query.MaxCount = MaxCount;

            return query;
        }

        internal TableQuery<ActiveRecord> Clone()
        {
            return Clone(Condition);
        }
    }
}
