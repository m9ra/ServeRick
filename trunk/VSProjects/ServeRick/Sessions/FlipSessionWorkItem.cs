using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Responsing;
using ServeRick.Processing;

namespace ServeRick.Sessions
{
    class FlipSessionWorkItem : ResponseWorkItem
    {
        readonly string _sessionID;

        internal FlipSessionWorkItem(string sessionID)
        {
            _sessionID = sessionID;
        }

        internal override void Run()
        {
            SessionProvider.FlipFlash(Unit.Output, _sessionID);
            Complete();
        }
    }
}
