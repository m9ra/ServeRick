using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Networking;
using ServeRick.Processing;

namespace ServeRick.Database
{
    abstract class DatabaseWorkItem : WorkItem
    {
        internal readonly ProcessingUnit Unit;

        internal DatabaseWorkItem(ProcessingUnit unit)
        {
            Unit = unit;
            PlanProcessor(unit.Database);
        }
    }
}
