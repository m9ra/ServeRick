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

        public override void ExecuteRow<ActiveRecord>(DataTable<ActiveRecord> table, TableQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
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
                    throw new NotImplementedException("Resolve conditions");
            }

            executor(result);
        }

        public override void ExecuteRows<ActiveRecord>(DataTable<ActiveRecord> table, TableQuery<ActiveRecord> query, RowsExecutor<ActiveRecord> executor)
        {
            IEnumerable<ActiveRecord> rows = table.MemoryRecords.Values;

            foreach (var item in query.Condition)
            {
                rows = applyCondition(table, rows, item);
            }

            rows = rows.Skip(query.Start).Take(query.MaxCount);
            var result = new RowsResult<ActiveRecord>(rows, table.MemoryRecords.Count);

            executor(result);
        }

        #endregion

        #region Private utilities

        private IEnumerable<ActiveRecord> applyCondition<ActiveRecord>(DataTable<ActiveRecord> table, IEnumerable<ActiveRecord> rows, WhereItem where)
            where ActiveRecord : DataRecord
        {
            foreach (var row in rows)
            {
                var column = table.GetColumn(where.Column, row);

                switch (where.Operation)
                {
                    case WhereOperation.Equal:
                        if (column == where.Operand)
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
