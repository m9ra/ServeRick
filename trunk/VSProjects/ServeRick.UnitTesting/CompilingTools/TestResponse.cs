using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ServeRick.UnitTesting.CompilingTools
{
    class TestResponse : Response
    {

        internal string WrittenData()
        {
            var output = new StringBuilder();

            foreach (var chunk in _toSend)
            {
                output.Append(Encoding.UTF8.GetString(chunk));
            }

            return output.ToString();
        }
    }
}
