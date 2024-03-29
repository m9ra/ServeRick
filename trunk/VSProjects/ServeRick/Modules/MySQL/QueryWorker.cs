﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Threading;

using MySql.Data;
using MySql.Data.MySqlClient;

using ServeRick.Database;

namespace ServeRick.Modules.MySQL
{
    /// <summary>
    /// Definition of work on query workers
    /// </summary>
    /// <param name="worker">Worker available </param>
    delegate void QueryWork(QueryWorker worker);

    class QueryWorker : DataDriver
    {
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// Connection used by worker
        /// </summary>
        private MySqlConnection _connection;

        /// <summary>
        /// Owning driver which work is handled by current worker
        /// </summary>
        private readonly MySqlDriver _owner;

        /// <summary>
        /// Thread used by current worker
        /// </summary>
        private readonly Thread _thread;

        internal QueryWorker(MySqlDriver driver)
        {
            _owner = driver;
            _thread = new Thread(_run);
            _thread.Start();
        }

        /// <summary>
        /// Run in separated thread and handle owners work
        /// </summary>
        private void _run()
        {
            prepareWorker();

            for (;;)
            {
                var work = _owner.DequeueWork();
                if (work == null)
                {
                    break;
                }

                RETRY:
                try
                {
                    work(this);
                }
                catch (MySqlException ex)
                {
                    var error = ex.Number;
                    Log.Error("MySqlException with Number: {0}, message: {1}", error, ex);

                    switch (error)
                    {
                        //TODO determine number for deadlock
                        case (int)MySqlErrorCode.LockDeadlock:
                            Log.Notice("Waiting for deadlock recover retry");
                            Thread.Sleep(500 + _rnd.Next(500));
                            //deadlock error occured
                            goto RETRY;

                        default:
                            throw;
                    }
                }
            }
        }

        /// <summary>
        /// Prepare connection used by worker
        /// </summary>
        private void prepareWorker()
        {
            _connection = new MySqlConnection(_owner.ConnectionString);
            _connection.Open();
        }

        private string getSqlOperation(WhereItem item, SqlQuery query)
        {
            string format;

            var operand = getSqlOperand(null, item.Operand);
            switch (item.Operation)
            {
                case WhereOperation.Equal:
                    if (operand == null)
                    {
                        format = "`{0}` IS {1}";
                    }
                    else
                    {
                        format = "`{0}` = {1}";
                    }
                    break;
                case WhereOperation.HasSubstring:
                    format = "`{0}` LIKE {1}";
                    operand = string.Format("%{0}%", operand);
                    break;
                case WhereOperation.IsSimilar:
                    format = "MATCH(`{0}`) AGAINST({1} IN BOOLEAN MODE)";
                    break;
                case WhereOperation.Lesser:
                    format = "`{0}` < {1}";
                    break;
                case WhereOperation.LesserOrEqual:
                    format = "`{0}` <= {1}";
                    break;
                case WhereOperation.Greater:
                    format = "`{0}` > {1}";
                    break;
                case WhereOperation.GreaterOrEqual:
                    format = "`{0}` >= {1}";
                    break;
                default:
                    throw new NotSupportedException("Cannot process operation" + item.Operation);
            }

            var column = item.Column;
            var operandHolder = query.CreateParameter(column, operand);

            return string.Format(format, column, operandHolder);
        }

        private object getSqlOperand(Column column, object netValue)
        {
            if (netValue == null)
                return null;

            var type = netValue.GetType();

            if (type.IsEnum)
                return (int)netValue;

            if (type == typeof(bool))
                return (bool)netValue ? 1 : 0;

            return netValue;
        }

        private string sqlType(Column column)
        {
            var type = column.Type;

            //TODO refactor
            if (type == typeof(string))
            {
                return "VARCHAR(150)";
            }

            if (type == typeof(long))
            {
                return "BIGINT";
            }

            if (type == typeof(ulong))
            {
                return "BIGINT UNSIGNED";
            }

            if (type == typeof(int) || type.IsEnum)
            {
                return "INT";
            }

            if (type == typeof(bool))
            {
                return "BOOL";
            }

            if (type == typeof(TimeSpan))
            {
                return "TIME";
            }

            if (type == typeof(DateTime))
            {
                return "DATETIME";
            }

            if (type == typeof(double))
            {
                return "FLOAT";
            }

            if (type == typeof(Decimal))
            {
                return "DECIMAL(45,30)";
            }

            throw new NotImplementedException("sql type mapping for: " + type);
        }

        private string sqlOptions(Column column)
        {
            if (column.Name == nameof(DataRecord.ID))
                return "AUTO_INCREMENT PRIMARY KEY";

            return "";
        }

        public void Ping()
        {
            executeScalar("SELECT 1");
        }

        #region Driver processing methods

        public override void ExecuteRow<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
        {
            var queryCmd = createSelect<ActiveRecord>(table, query.Condition);

            queryCmd.Append(" LIMIT 1");

            ActiveRecord result = default(ActiveRecord);
            using (var reader = queryCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var columns = new ColumnsReader(reader);
                    result = table.CreateRecord(columns);
                }
                reader.Close();
            }

            executor(result);
        }

        public void ExecuteNative(string sql)
        {
            var queryCmd = getQuery();
            queryCmd.Append(sql.Trim());
            var scalarResult = queryCmd.ExecuteNonQuery();
        }

        public override void ExecuteRows<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowsExecutor<ActiveRecord> executor)
        {
            var queryCmd = createSelect(table, query.Condition, true);
            appendOrderBy(queryCmd, query.Ordering);
            appendLimit(queryCmd, query);

            var results = new List<ActiveRecord>();
            using (var reader = queryCmd.ExecuteReader())
            {
                var columns = new ColumnsReader(reader);
                
                while (reader.Read())
                {
                    var result = table.CreateRecord(columns);
                    results.Add(result);
                }
                reader.Close();
            }

            var totalRowsScalar = executeScalar("SELECT FOUND_ROWS()");
            var totalRows = Convert.ToInt32(totalRowsScalar);

            var rowsResult = new RowsResult<ActiveRecord>(results, totalRows);
            executor(rowsResult);
        }

        public override void InsertRows<ActiveRecord>(DataTable<ActiveRecord> table, InsertQuery<ActiveRecord> query, InsertExecutor<ActiveRecord> executor)
        {
            var inserted = new List<ActiveRecord>();

            foreach (var row in query.Rows)
            {
                var insertCmd = getQuery();
                insertCmd.AppendFormat("INSERT INTO `{0}`(", table.Name);

                var values = new StringBuilder("VALUES(");
                var isFirst = true;
                foreach (var column in table.Columns)
                {
                    if (column.Name == nameof(row.ID))
                        //id is not inserted
                        continue;

                    if (!isFirst)
                    {
                        insertCmd.Append(",");
                        values.Append(",");
                    }

                    insertCmd.AppendFormat("`{0}`", column.Name);

                    var value = table.GetColumnValue(column.Name, row);
                    var parameter = insertCmd.CreateParameter(column.Name, value);
                    values.Append(parameter);


                    isFirst = false;
                }
                insertCmd.Append(")");
                values.Append(")");
                insertCmd.Append(values.ToString());

                insertCmd.Append("; SELECT last_insert_id();");

                //back retrieve inserted row id
                var scalar = insertCmd.ExecuteScalar();
                table.SetColumnValue(row, nameof(row.ID), Convert.ToInt32(scalar));
                inserted.Add(row);
            }

            executor(inserted);
        }

        public override void UpdateRows<ActiveRecord>(DataTable<ActiveRecord> table, UpdateQuery<ActiveRecord> query, UpdateExecutor<ActiveRecord> executor)
        {
            var queryCmd = getQuery();

            queryCmd.AppendFormat("UPDATE `{0}` SET ", table.Name);

            var isFirst = true;
            foreach (var update in query.Updates)
            {
                if (!isFirst)
                {
                    queryCmd.Append(",");
                }

                var columnName = update.Key;
                var column = table.GetColumn(columnName);
                var operand = getSqlOperand(column, update.Value);

                var parameter = queryCmd.CreateParameter(columnName, operand);
                queryCmd.AppendFormat(" `{0}` = {1} ", columnName, parameter);

                isFirst = false;
            }

            queryCmd.Append(" WHERE ");
            appendCondition(queryCmd, query.Select.Condition);

            appendLimit(queryCmd, query.Select);

            var updatedRows = queryCmd.ExecuteNonQuery();
            executor(updatedRows);
        }

        public override void RemoveRows<ActiveRecord>(DataTable<ActiveRecord> table, RemoveQuery<ActiveRecord> query, Action executor)
        {
            var queryCmd = getQuery();
            queryCmd.AppendFormat("DELETE FROM {0} WHERE ", table.Name);

            appendCondition(queryCmd, query.Select.Condition);
            queryCmd.ExecuteNonQuery();

            executor();
        }

        public override void Initialize<ActiveRecord>(DataTable<ActiveRecord> table)
        {
            var query = getQuery();

            query.Append("CREATE TABLE IF NOT EXISTS ");
            query.Append(table.Name);

            query.Append("(");
            var columns = table.Columns;
            appendColumnDefinitions(query, columns);
            query.Append(")");

            query.ExecuteNonQuery();
        }

        public override void Call<ActiveRecord>(DataTable<ActiveRecord> table, CallQuery<ActiveRecord> query, Action executor)
        {
            var queryCmd = getQuery();

            queryCmd.Append(query.CallName);
            queryCmd.MarkProcedure();

            addCallArguments(queryCmd, query.Arguments);

            queryCmd.ExecuteNonQuery();

            executor();
        }

        #endregion

        #region MySql command execution primitives

        /// <summary>
        /// Execute query as scalar
        /// </summary>
        /// <param name="scalarQuery">Query evaluated as scalar query</param>
        /// <returns>Result of query evaluation</returns>
        private object executeScalar(string scalarQuery)
        {
            var queryCmd = getQuery();
            queryCmd.Append(scalarQuery);

            var scalarResult = queryCmd.ExecuteScalar();
            return scalarResult;
        }

        /// <summary>
        /// Blockingly get command (with possible reconnection)
        /// </summary>
        /// <returns>Created command</returns>
        private SqlQuery getQuery()
        {
            var cmd = new SqlQuery(_connection);
            return cmd;
        }

        #endregion

        #region MySql command primitives

        /// <summary>
        /// Append definitions of given columns into query
        /// </summary>
        /// <param name="query">Query where definitions will be appended</param>
        /// <param name="columns">Columns which definitions will be appended</param>
        private void appendColumnDefinitions(SqlQuery query, IEnumerable<Column> columns)
        {
            var first = true;
            foreach (var column in columns)
            {
                if (!first)
                    query.AppendLine(",");

                var typeRepresentation = sqlType(column);
                var columnOptions = sqlOptions(column);

                query.AppendFormat("`{0}` {1} {2}", column.Name, typeRepresentation, columnOptions);

                first = false;
            }
        }

        /// <summary>
        /// Append condition in sql form into query
        /// </summary>
        /// <param name="query">Query where condition will be appended</param>
        /// <param name="condition">Condition which sql representation will be appended</param>
        private void appendCondition(SqlQuery query, WhereClause condition)
        {
            var isFirst = true;
            foreach (var item in condition)
            {
                if (!isFirst)
                {
                    query.Append(" AND ");
                }

                var operation = getSqlOperation(item, query);

                query.Append(operation);
                isFirst = false;
            }

            var isEmpty = isFirst;
            if (isEmpty)
            {
                query.Append(" 1 ");
            }
        }

        /// <summary>
        /// Add arguments for CALL to given command
        /// </summary>
        /// <param name="queryCmd">Query where arguments will be added</param>
        /// <param name="arguments">Arguments that will be added</param>
        private void addCallArguments(SqlQuery queryCmd, IEnumerable<KeyValuePair<string, object>> arguments)
        {
            foreach (var argument in arguments)
            {
                queryCmd.CreateParameter(argument.Key, argument.Value);
            }
        }

        private void appendLimit<ActiveRecord>(SqlQuery queryCmd, SelectQuery<ActiveRecord> select)
            where ActiveRecord : DataRecord
        {
            if (select.MaxCount != int.MaxValue)
            {
                //there is limit on maximal rows count
                if (select.Start == 0)
                {
                    queryCmd.AppendFormat(" LIMIT {0} ", select.MaxCount);
                }
                else
                {
                    queryCmd.AppendFormat(" LIMIT {0},{1} ", select.Start, select.MaxCount);
                }
            }
        }


        private void appendOrderBy(SqlQuery queryCmd, OrderByClause ordering)
        {
            //TODO add ordering API
            if (ordering.IsRandom)
            {
                queryCmd.Append(" ORDER BY rand() ");
            }
            else
            {
                queryCmd.Append(" ORDER BY ID DESC ");
            }
        }

        /// <summary>
        /// Create select command on given table and condition
        /// </summary>
        /// <typeparam name="ActiveRecord">Type of selected active record</typeparam>
        /// <param name="table">Table which active which active records are selected</param>
        /// <param name="condition">Condition of select query</param>
        /// <param name="countRows">Determine that select should count total results</param>
        /// <returns>Created query</returns>
        private SqlQuery createSelect<ActiveRecord>(DataTable<ActiveRecord> table, WhereClause condition, bool countRows = false)
            where ActiveRecord : DataRecord
        {
            var query = getQuery();
            query.Append("SELECT");

            if (countRows)
                query.Append(" SQL_CALC_FOUND_ROWS ");

            query.AppendFormat(" * FROM `{0}` WHERE ", table.Name);

            appendCondition(query, condition);
            return query;
        }

        #endregion


    }
}
