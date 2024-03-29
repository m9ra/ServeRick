﻿using System;
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
        private readonly Dictionary<string, Column> _columns = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, FieldInfo> _columnFields = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Driver handling data in current table.
        /// </summary>
        internal readonly DataDriver Driver;

        /// <summary>
        /// Type of records stored in table
        /// </summary>
        public readonly Type RecordType = typeof(ActiveRecord);

        /// <summary>
        /// ActiveRecords stored in memory. Can be used for caching purposes or for memory storages.
        /// </summary>
        public readonly Dictionary<int, ActiveRecord> MemoryRecords = new Dictionary<int, ActiveRecord>();

        /// <summary>
        /// Name of table, used by driver
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Columns defined on current table
        /// </summary>
        public IEnumerable<Column> Columns { get { return _columns.Values; } }

        public DataTable(DataDriver driver, string name = null)
        {
            Driver = driver;
            Name = name == null ? typeof(ActiveRecord).Name : name;

            foreach (var field in RecordType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!field.IsInitOnly)
                    //only read only fields are treated as database columns
                    continue;

                var column = new Column(field.Name, field.FieldType);
                _columns.Add(column.Name, column);
                _columnFields.Add(column.Name, getField(column.Name));
            }

            driver.Initialize(this);
        }

        internal override Type GetRecordType()
        {
            return typeof(ActiveRecord);
        }


        internal Column GetColumn(string columnName)
        {
            return _columns[columnName];
        }

        internal object GetColumnValue(string column, ActiveRecord record)
        {
            var field = _columnFields[column];
            return field.GetValue(record);
        }

        internal void SetColumnValue(ActiveRecord record, string column, object value)
        {
            var field = _columnFields[column];
            field.SetValue(record, value);
        }

        internal ActiveRecord CreateRecord(ColumnsReaderBase reader)
        {
            var record = Activator.CreateInstance<ActiveRecord>();

            foreach (var column in Columns)
            {
                var type = column.Type;
                object value = null;
                reader.SetColumn(column);

                if (type == typeof(string))
                {
                    value = reader.ReadString();
                }
                else if (type == typeof(int))
                {
                    value = reader.ReadInt();
                }
                else if (type.IsEnum)
                {
                    value = reader.ReadEnum();
                }
                else if (type == typeof(double))
                {
                    value = reader.ReadDouble();
                }
                else if (type == typeof(bool))
                {
                    value = reader.ReadBool();
                }
                else if (type == typeof(DateTime))
                {
                    value = reader.ReadDateTime();
                }
                else if (type == typeof(TimeSpan))
                {
                    value = reader.ReadTimeSpan();
                }
                else if (type == typeof(long))
                {
                    value = reader.ReadInt64();
                }
                else if (type == typeof(ulong))
                {
                    value = reader.ReadUInt64();
                }
                else if (type == typeof(decimal))
                {
                    value = reader.ReadDecimal();
                }
                else
                {
                    throw new NotImplementedException("Reader for type " + type + " is missing");
                }

                SetColumnValue(record, column.Name, value);
            }

            return record;
        }


        private FieldInfo getField(string column)
        {
            var type = typeof(ActiveRecord);
            var field = type.GetField(column, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            return field;
        }

    }
}
