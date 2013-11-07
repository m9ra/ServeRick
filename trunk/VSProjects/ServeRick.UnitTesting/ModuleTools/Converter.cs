using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.UnitTesting.ModuleTools
{
    static class Converter
    {
        internal static byte[] GetInputBytes(string data)
        {
            //TODO resolve encoding that is really used by browsers
            return Encoding.ASCII.GetBytes(data);
        }
    }
}
