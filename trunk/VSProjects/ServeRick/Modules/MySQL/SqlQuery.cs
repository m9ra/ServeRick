using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace ServeRick.Modules.MySQL
{
    class SqlQuery
    {
        private readonly MySqlCommand _command;

        private readonly StringBuilder _text = new StringBuilder();

        internal SqlQuery(MySqlConnection connection)
        {
            _command = new MySqlCommand();
            _command.Connection = connection;
        }

        internal void AddWithValue(string paramName, object value)
        {
            _command.Parameters.AddWithValue(paramName, value);
        }

        internal void AppendFormat(string format, params object[] formatArgs)
        {
            _text.AppendFormat(format, formatArgs);
        }

        internal void Append(string data)
        {
            _text.Append(data);
        }

        internal void AppendLine(string line)
        {
            _text.AppendLine(line);
        }

        internal MySqlDataReader ExecuteReader()
        {
            prepareQuery();
            return _command.ExecuteReader();
        }

        internal object ExecuteScalar()
        {
            prepareQuery();
            return _command.ExecuteScalar();
        }

        internal void ExecuteNonQuery()
        {
            prepareQuery();
            _command.ExecuteNonQuery();
        }

        private void prepareQuery()
        {
            Log.Notice("MySql:{0}", _text);
            _command.CommandText = _text.ToString();
        }
    }
}
