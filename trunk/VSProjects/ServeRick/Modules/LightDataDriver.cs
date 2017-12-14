using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Database;

namespace ServeRick.Modules
{
    public delegate void LightDataDriveCall<ActiveRecord>(CallTableApi<ActiveRecord> table, Dictionary<string, object> values)
        where ActiveRecord : DataRecord;

    public class CallTableApi<ActiveRecord>
        where ActiveRecord : DataRecord
    {
        private readonly DataTable<ActiveRecord> _table;

        internal CallTableApi(DataTable<ActiveRecord> table)
        {
            _table = table;
        }

        public void SetColumnValue(ActiveRecord row, string column, object value)
        {
            _table.SetColumnValue(row, column, value);
        }

        public ActiveRecord GetRecord(int id)
        {
            return _table.MemoryRecords[id];
        }

        public object GetColumnValue(string column, ActiveRecord row)
        {
            return _table.GetColumnValue(column, row);
        }
    }

    /// <summary>
    /// Light implementation of data driver, storing it's values in memory.
    /// Is supposed to be used as development only driver.
    /// </summary>
    public class LightDataDriver : DataDriver
    {
        private readonly DataRecord[] _records;

        private readonly Dictionary<string, object> _storedCalls = new Dictionary<string, object>();

        public LightDataDriver(IEnumerable<DataRecord> records)
        {
            _records = records.ToArray();
        }

        public LightDataDriver(params DataRecord[] records) :
            this((IEnumerable<DataRecord>)records)
        {

        }

        public void AddCall<ActiveRecord>(string callName, LightDataDriveCall<ActiveRecord> callHandler)
            where ActiveRecord : DataRecord
        {
            _storedCalls.Add(callName, callHandler);
        }

        #region Data driver API implementation

        public override void Initialize<ActiveRecord>(DataTable<ActiveRecord> table)
        {
            foreach (var record in _records)
            {
                if (!(record is ActiveRecord))
                    continue;

                table.MemoryRecords[record.ID] = record as ActiveRecord;
            }
        }

        public override void ExecuteRow<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
        {
            var items = query.Condition.ToArray();

            ActiveRecord result;
            switch (items.Length)
            {
                case 0:
                    result = table.MemoryRecords.Values.First();
                    break;
                case 1:
                    result = findItem(table, items[0]);
                    break;
                default:
                    result = applyCondition(table, query.Condition).FirstOrDefault();
                    break;
            }

            executor(result);
        }

        public override void ExecuteRows<ActiveRecord>(DataTable<ActiveRecord> table, SelectQuery<ActiveRecord> query, RowsExecutor<ActiveRecord> executor)
        {
            var rows = applyCondition(table, query.Condition);

            rows = rows.Skip(query.Start).Take(query.MaxCount);
            var result = new RowsResult<ActiveRecord>(rows, table.MemoryRecords.Count);

            executor(result);
        }

        public override void InsertRows<ActiveRecord>(DataTable<ActiveRecord> table, InsertQuery<ActiveRecord> query, InsertExecutor<ActiveRecord> executor)
        {
            foreach (var row in query.Rows)
            {
                table.SetColumnValue(row, nameof(row.ID), getUID(table));
                table.MemoryRecords.Add(row.ID, row);
            }

            executor?.Invoke(query.Rows);
        }

        public override void RemoveRows<ActiveRecord>(DataTable<ActiveRecord> table, RemoveQuery<ActiveRecord> query, Action executor)
        {
            ExecuteRows(table, query.Select, (res) =>
            {
                foreach (var row in res.Rows)
                {
                    table.MemoryRecords.Remove(row.ID);
                }
            });

            executor();
        }

        public override void UpdateRows<ActiveRecord>(DataTable<ActiveRecord> table, UpdateQuery<ActiveRecord> query, UpdateExecutor<ActiveRecord> executor)
        {
            int updates = 0;
            ExecuteRows(table, query.Select, (res) =>
            {
                foreach (var row in res.Rows)
                {
                    foreach (var update in query.Updates)
                    {
                        ++updates;
                        table.SetColumnValue(row, update.Key, update.Value);
                    }
                }
            });

            executor(updates);
        }

        public override void Call<ActiveRecord>(DataTable<ActiveRecord> table, CallQuery<ActiveRecord> query, Action executor)
        {
            var callHandler = _storedCalls[query.CallName] as LightDataDriveCall<ActiveRecord>;
            callHandler(new CallTableApi<ActiveRecord>(table), query.Arguments.ToDictionary(p => p.Key, p => p.Value));
            executor();
        }

        #endregion

        #region Private utilities

        private int getUID<ActiveRecord>(DataTable<ActiveRecord> table)
            where ActiveRecord : DataRecord
        {
            if (!table.MemoryRecords.Any())
                return 0;

            return table.MemoryRecords.Keys.Max() + 1;
        }

        private IEnumerable<ActiveRecord> applyItem<ActiveRecord>(DataTable<ActiveRecord> table, IEnumerable<ActiveRecord> rows, WhereItem where)
            where ActiveRecord : DataRecord
        {
            foreach (var row in rows)
            {
                var column = table.GetColumnValue(where.Column, row);

                switch (where.Operation)
                {
                    case WhereOperation.Equal:
                        if (column.Equals(where.Operand))
                            yield return row;
                        break;

                    case WhereOperation.IsSimilar:
                    //TODO implement
                    case WhereOperation.HasSubstring:
                        var value = column as string;
                        if (value.Contains(where.Operand.ToString()))
                            yield return row;
                        break;
                    case WhereOperation.GreaterOrEqual:
                        if(column is long)
                        {
                            var longValue = (long)column;
                            if (longValue >= (long)where.Operand)
                                yield return row;
                            break;
                        }
                        throw new NotImplementedException("Operand");

                    case WhereOperation.Greater:
                        if (column is int)
                        {
                            var longValue = (int)column;
                            if (longValue > (int)where.Operand)
                                yield return row;
                            break;
                        }
                        throw new NotImplementedException("Operand");
                    default:
                        throw new NotImplementedException("Operation");
                }
            }
        }

        private IEnumerable<ActiveRecord> applyCondition<ActiveRecord>(DataTable<ActiveRecord> table, WhereClause query)
            where ActiveRecord : DataRecord
        {
            IEnumerable<ActiveRecord> rows = table.MemoryRecords.Values;

            foreach (var item in query)
            {
                rows = applyItem(table, rows, item);
            }

            return rows;
        }

        private ActiveRecord findItem<ActiveRecord>(DataTable<ActiveRecord> table, WhereItem condition)
            where ActiveRecord : DataRecord
        {
            ActiveRecord result;

            if (condition.Operation != WhereOperation.Equal || condition.Column != "ID")
            {
                result = null;
                var type = typeof(ActiveRecord);
                var field = type.GetField(condition.Column);
                if (field == null)
                    throw new InvalidOperationException("Unknown field: " + condition.Column);

                foreach (var record in table.MemoryRecords.Values)
                {
                    var value = field.GetValue(record);
                    if (condition.Operand.Equals(value))
                    {
                        result = record;
                        break;
                    }
                }
            }
            else
            {
                table.MemoryRecords.TryGetValue((int)condition.Operand, out result);
            }

            return result;
        }

        #endregion
    }
}
