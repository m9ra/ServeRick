using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace SharpServer
{
    public abstract class Controller
    {
        protected Response Response { get; private set; }

        protected ControllerManager Manager { get; private set; }

        internal void SetResponse(ControllerManager manager, Response response)
        {
            Response = response;
            Manager = manager;  
        }

        protected void Render(string fileName)
        {
            var handler=Manager.GetFileHandler(fileName);
            if (handler == null)
            {
                throw new KeyNotFoundException("Handler for file: " + fileName);
            }
            Response.Render(handler);
        }
    }
}
