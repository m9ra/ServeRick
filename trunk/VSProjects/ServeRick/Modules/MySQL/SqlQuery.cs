using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

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

        internal string AddParameter(string paramHint, object value)
        {
            var parameter = "@" + paramHint;
            var number = 0;
            while (_command.Parameters.Contains(parameter))
            {
                ++number;
                parameter = "@" + paramHint + number;
            }
            _command.Parameters.AddWithValue(parameter, value);
            _command.Parameters[parameter].Direction = ParameterDirection.Input;

            return parameter;
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

        internal int ExecuteNonQuery()
        {
            prepareQuery();
            return _command.ExecuteNonQuery();
        }

        private void prepareQuery()
        {
            var text = _text.ToString().Trim();
            Log.Notice("MySql[{2}]:{0} | {1}", text, parametersToString(), _command.Connection.GetHashCode());
            _command.CommandText = text;
        }

        private string parametersToString()
        {
            var result = new StringBuilder();
            foreach (MySqlParameter param in _command.Parameters)
            {
                if (result.Length > 0)
                    result.Append(", ");
                result.Append(param.ParameterName);
                result.Append('=');
                result.Append(param.Value);
            }
            return result.ToString();
        }

        internal void MarkProcedure()
        {
            _command.CommandType = CommandType.StoredProcedure;
            _command.EnableCaching = false;
        }
    }
}
