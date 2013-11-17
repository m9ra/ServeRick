using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Responsing;
using ServeRick.Processing;

namespace ServeRick.Sessions
{
    class FlipSessionWorkItem : WorkItem
    {
        readonly OutputProcessor _output;

        readonly string _sessionID;

        internal FlipSessionWorkItem(ProcessingUnit unit, string sessionID)
        {
            _output = unit.Output;
            _sessionID = sessionID;

            PlanProcessor(_output);
        }

        internal override void Run()
        {
            SessionProvider.FlipFlash(_output, _sessionID);
        }
    }
}
