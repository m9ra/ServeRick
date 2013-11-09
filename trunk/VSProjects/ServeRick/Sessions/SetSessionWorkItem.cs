using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Responsing;
using ServeRick.Processing;

namespace ServeRick.Sessions
{
    class SetSessionWorkItem : WorkItem
    {
        readonly OutputProcessor _output;

        readonly string _sessionID;

        readonly object _storedData;

        internal SetSessionWorkItem(ProcessingUnit unit, string sessionID, object storedData)
        {
            _output = unit.Output;
            _sessionID = sessionID;
            _storedData = storedData;

            PlanProcessor(_output);
        }

        internal override void Run()
        {
            _output.Sessions[_sessionID] = _storedData;
        }
    }
}
