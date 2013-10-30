using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;
using ServeRick.Networking;

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
