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
        protected override void onAbort()
        {
            //TODO: how to abort database items
        }

        internal override WorkProcessor PlannedProcessor
        {
            get { return Unit.Database; }
        }
    }
}
