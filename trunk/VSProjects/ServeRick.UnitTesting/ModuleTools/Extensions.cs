using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.UnitTesting.ModuleTools
{
    static class Extensions
    {
        public static MultiPartTest Boundary(this string input, string delimiter)
        {
            return new MultiPartTest(input, delimiter);
        }
    }
}
