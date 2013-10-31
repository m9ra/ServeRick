using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    public abstract class DataTable
    {
        /// <summary>
        /// Avoid subclassing from outer assemblies
        /// </summary>
        internal DataTable()
        { }

        abstract internal Type GetRecordType();

        public static DataTable Create<ActiveRecord>(DataDriver<ActiveRecord> drive)
            where ActiveRecord : DataRecord
        {
            return new DataTable<ActiveRecord>(drive);
        }
    }

    public class DataTable<ActiveRecord> : DataTable
        where ActiveRecord : DataRecord
    {
        internal readonly DataDriver<ActiveRecord> Driver;

        internal DataTable(DataDriver<ActiveRecord> driver)
        {
            Driver = driver;
        }

        internal override Type GetRecordType()
        {
            return typeof(ActiveRecord);
        }
    }
}
