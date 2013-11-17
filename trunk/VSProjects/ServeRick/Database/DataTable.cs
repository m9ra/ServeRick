using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

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
    }

    public class DataTable<ActiveRecord> : DataTable
        where ActiveRecord : DataRecord
    {
        /// <summary>
        /// Driver handling data in current table.
        /// </summary>
        internal readonly DataDriver Driver;

        /// <summary>
        /// ActiveRecords stored in memory. Can be used for caching purposes or for memory storages.
        /// </summary>
        public readonly Dictionary<int, ActiveRecord> MemoryRecords = new Dictionary<int, ActiveRecord>();

        public DataTable(DataDriver driver)
        {
            Driver = driver;

            driver.Initialize(this);
        }

        internal override Type GetRecordType()
        {
            return typeof(ActiveRecord);
        }

        internal object GetColumn(string column, ActiveRecord record)
        {
            var field = getField(column);
            return field.GetValue(record);
        }

        internal void SetColumn(ActiveRecord record, string column, object value)
        {
            var field = getField(column);
            field.SetValue(record, value);
        }

        private FieldInfo getField(string column)
        {
            var type = typeof(ActiveRecord);
            var field = type.GetField(column, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            return field;
        }
    }
}
