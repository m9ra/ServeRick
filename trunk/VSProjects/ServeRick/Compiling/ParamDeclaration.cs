using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Compiling
{
    public class ParamDeclaration
    {
        public readonly string Name;

        public readonly Type Type;

        internal ParamDeclaration(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
