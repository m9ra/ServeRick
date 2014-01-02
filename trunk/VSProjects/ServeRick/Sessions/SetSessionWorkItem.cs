using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Responsing;
using ServeRick.Processing;

namespace ServeRick.Sessions
{
    class SetSessionWorkItem : ResponseWorkItem
    {
        readonly string _sessionID;

        readonly object _storedData;

        internal SetSessionWorkItem(string sessionID, object storedData)
        {
            _sessionID = sessionID;
            _storedData = storedData;
        }

        internal override void Run()
        {
            SessionProvider.SetData(Unit.Output, _sessionID, _storedData);
            Complete();
        }

        protected override void onAbort()
        {
            //nothing to be aborted
        }
    }
}
