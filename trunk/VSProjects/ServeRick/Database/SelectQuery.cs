﻿using System;
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
    public abstract class SelectQuery
    {
        internal SelectQuery()
        {
            //prevent creating from another assemblies
        }
    }

    /// <summary>
    /// Query on table representation.
    /// </summary>
    /// <typeparam name="ActiveRecord">Tyep of active record.</typeparam>
    public class SelectQuery<ActiveRecord> : SelectQuery
        where ActiveRecord : DataRecord
    {
        internal readonly Response Response;

        public readonly WhereClause Condition;

        public int MaxCount { get; private set; }

        public int Start { get; private set; }

        internal SelectQuery(Response response)
            : this(response, new WhereClause())
        {
            MaxCount = int.MaxValue;
        }

        internal SelectQuery(Response response, WhereClause where)
        {
            Response = response;
            Condition = where;
        }

        public SelectQuery<ActiveRecord> Find(int id)
        {
            return Where("id", WhereOperation.Equal, id).Limit(1);
        }

        public SelectQuery<ActiveRecord> WhereEqual(string column, object operand)
        {
            return Where(column, WhereOperation.Equal, operand);
        }

        public SelectQuery<ActiveRecord> Where(string column, WhereOperation operation, object operand)
        {
            var item = new WhereItem(column, operation, operand);
            var conditon = Condition.Clone();

            conditon.Add(item);
            var query = Clone(conditon);
            return query;
        }

        public UpdateQuery<ActiveRecord> Update(string column, object value)
        {
            var update = new UpdateQuery<ActiveRecord>(this);

            return update.Update(column, value);
        }

        public SelectQuery<ActiveRecord> Limit(int maxCount)
        {
            var clonned = Clone();
            clonned.MaxCount = maxCount;
            return clonned;
        }

        public SelectQuery<ActiveRecord> Offset(int start)
        {
            var clonned = Clone();
            clonned.Start = start;
            return clonned;
        }

        public void ExecuteRow(RowExecutor<ActiveRecord> executor)
        {
            var work = new RowQueryWorkItem<ActiveRecord>(this, executor);
            Response.Client.EnqueueWork(work);
        }

        public void ExecuteRows(RowsExecutor<ActiveRecord> executor)
        {
            var work = new RowsQueryWorkItem<ActiveRecord>(this, executor);
            Response.Client.EnqueueWork(work);
        }

        internal SelectQuery<ActiveRecord> Clone(WhereClause where)
        {
            var query = new SelectQuery<ActiveRecord>(Response, where);
            query.Start = Start;
            query.MaxCount = MaxCount;

            return query;
        }

        internal SelectQuery<ActiveRecord> Clone()
        {
            return Clone(Condition);
        }
    }
}