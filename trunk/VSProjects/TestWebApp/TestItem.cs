using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Database;

namespace TestWebApp
{
    class TestItem:DataRecord
    {
        public string Data { get; set; }

        internal TestItem(int id, string data)
        {
            ID = id;
            Data = data;
        }
    }
}
