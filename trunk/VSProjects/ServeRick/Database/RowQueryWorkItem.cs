using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Database
{
    class RowQueryWorkItem<ActiveRecord>: ClientWorkItem
        where ActiveRecord : DataRecord
    {
        /// <summary>
        /// Query stored for work item.
        /// </summary>
        private readonly TableQuery<ActiveRecord> _query;

        /// <summary>
        /// Executor processed with retrieved row.
        /// </summary>
        private readonly RowExecutor<ActiveRecord> _executor;

        internal RowQueryWorkItem(TableQuery<ActiveRecord> query, RowExecutor<ActiveRecord> executor)
        {
            _query = query;
            _executor = executor;
        }

        protected override WorkProcessor getPlannedProcessor()
        {
            return Unit.Database;
        }

        internal override void Run()
        {
            var driver = Unit.Database.GetDriver<ActiveRecord>();

            driver.ExecuteRow(_query, _handler);
        }

        /// <summary>
        /// Row execution handler, is called asynchronously, when 
        /// query execution is done.
        /// </summary>
        /// <param name="row">Retrieved row.</param>
        private void _handler(ActiveRecord row) {
            _executor(row);
            Complete();
        }
    }
}
