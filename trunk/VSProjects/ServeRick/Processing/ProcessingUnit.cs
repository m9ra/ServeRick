using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Database;
using ServeRick.Responsing;

namespace ServeRick.Processing
{
    class ProcessingUnit
    {
        internal readonly DatabaseProcessor Database;

        internal readonly OutputProcessor Output;

        internal ProcessingUnit()
        {
            Output = new OutputProcessor();
            Database = new DatabaseProcessor();
        }
    }
}
