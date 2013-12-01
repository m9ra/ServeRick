using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    public class Column
    {
        public readonly string Name;

        public readonly Type Type;

        public Column(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
