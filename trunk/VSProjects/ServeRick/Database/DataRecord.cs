using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    public abstract class DataRecord
    {
        public readonly int ID;

        protected DataRecord() { }

        protected DataRecord(int id)
        {
            ID = id;
        }
    }
}
