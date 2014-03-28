using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data;
using MySql.Data.MySqlClient;

using ServeRick.Database;

namespace ServeRick.Modules.MySQL
{
    class ColumnsReader : ColumnsReaderBase
    {
        private readonly MySqlDataReader _reader;

        internal ColumnsReader(MySqlDataReader reader)
        {
            _reader = reader;
        }

        public override string ReadString()
        {
            return _reader.GetString(ColumnName);
        }

        public override int ReadInt()
        {
            return _reader.GetInt32(ColumnName);
        }

        public override int ReadEnum()
        {
            var value = _reader.GetString(ColumnName);
            var parsed = (int)Enum.Parse(Column.Type, value);

            return parsed;
        }

        public override bool ReadBool()
        {
            return _reader.GetBoolean(ColumnName);
        }

        public override DateTime ReadDateTime()
        {
            return _reader.GetDateTime(ColumnName);
        }

        public override TimeSpan ReadTimeSpan()
        {
            return _reader.GetTimeSpan(ColumnName);
        }

        public override double ReadDouble()
        {
            return _reader.GetDouble(ColumnName);
        }
    }
}
