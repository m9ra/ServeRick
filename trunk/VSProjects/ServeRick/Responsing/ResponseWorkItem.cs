using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Processing;

namespace ServeRick.Responsing
{
    abstract class ResponseWorkItem : WorkItem
    {
        internal override WorkProcessor PlannedProcessor
        {
            get { return Unit.Output; }
        }
    }
}
