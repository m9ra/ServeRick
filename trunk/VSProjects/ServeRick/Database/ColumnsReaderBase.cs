using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Database;

namespace ServeRick.Database
{
    public abstract class ColumnsReaderBase
    {
        /// <summary>
        /// Determine name of column that will be read
        /// </summary>
        public string ColumnName { get { return Column.Name; } }

        public Column Column { get; private set; }

        /// <summary>
        /// Read string value for given column
        /// </summary>
        /// <returns>String that has been read for column</returns>
        public abstract string ReadString();

        public abstract int ReadInt();

        public abstract int ReadEnum();

        public abstract bool ReadBool();

        public abstract DateTime ReadDateTime();

        public abstract TimeSpan ReadTimeSpan();

        public abstract double ReadDouble();
        

        internal void SetColumn(Column column)
        {
            Column = column;
        }

    }
}
