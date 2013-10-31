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
    public class LightDataDriver<ActiveRecord> : DataDriver<ActiveRecord>
        where ActiveRecord : DataRecord
    {
        private readonly Dictionary<int, ActiveRecord> _records = new Dictionary<int, ActiveRecord>();

        public LightDataDriver(IEnumerable<ActiveRecord> records)
        {
            foreach (var record in records)
            {
                _records.Add(record.ID, record);
            }
        }


        public override void ExecuteRow(TableQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
        {
            var items = query.Condition.ToArray();


            ActiveRecord result;
            switch (items.Length)
            {
                case 0:
                    result = _records.Values.First();
                    break;
                case 1:
                    result = findItem(items[0]);
                    break;
                default:
                    throw new NotImplementedException("Resolve conditions");
            }

            executor(result);
        }

        private ActiveRecord findItem(WhereItem condition)
        {
            if (condition.Operation != WhereOperation.Equal || condition.Column!="id")
                throw new NotImplementedException();

            ActiveRecord result;
            _records.TryGetValue((int)condition.Operand, out result);

            return result;
        }
    }
}
