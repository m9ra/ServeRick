using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Database;

namespace ServeRick.Modules
{
    /// <summary>
    /// Light implementation of data driver, storing it's values in memory.
    /// Is supposed to be used as development only driver.
    /// </summary>
    public class LightDataDriver : DataDriver
    {
        private readonly DataRecord[] _records;

        public LightDataDriver(IEnumerable<DataRecord> records)
        {
            _records = records.ToArray();
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
                table.SetColumn(row, "id", getUID(table));
                table.MemoryRecords.Add(row.ID, row);
            }

            executor(query.Rows);
        }



        public override void UpdateRows<ActiveRecord>(DataTable<ActiveRecord> table, UpdateQuery<ActiveRecord> query, Action executor)
        {
            ExecuteRows(table, query.Select, (res) =>
            {
                foreach (var row in res.Rows)
                {
                    foreach (var update in query.Updates)
                    {
                        table.SetColumn(row, update.Key, update.Value);
                    }
                }
            });

            executor();
        }

        #endregion

        #region Private utilities

        private int getUID<ActiveRecord>(DataTable<ActiveRecord> table)
            where ActiveRecord : DataRecord
        {
            return table.MemoryRecords.Keys.Max() + 1;
        }

        private IEnumerable<ActiveRecord> applyItem<ActiveRecord>(DataTable<ActiveRecord> table, IEnumerable<ActiveRecord> rows, WhereItem where)
            where ActiveRecord : DataRecord
        {
            foreach (var row in rows)
            {
                var column = table.GetColumn(where.Column, row);

                switch (where.Operation)
                {
                    case WhereOperation.Equal:
                        if (column.Equals(where.Operand))
                            yield return row;
                        break;
                    case WhereOperation.HasSubstring:
                        var value = column as string;
                        if (value.Contains(where.Operand.ToString()))
                            yield return row;
                        break;
                    default:
                        throw new NotImplementedException();
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
            if (condition.Operation != WhereOperation.Equal || condition.Column != "id")
                throw new NotImplementedException();

            ActiveRecord result;
            table.MemoryRecords.TryGetValue((int)condition.Operand, out result);

            return result;
        }

        #endregion
    }
}
