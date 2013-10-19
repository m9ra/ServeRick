using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer;
using SharpServer.Networking;

namespace TestWebApp
{
    class InputManager:InputManagerBase
    {
        protected override InputController createController(HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
