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
        private ResponseHandler _layout = null;

        protected Response Response { get; private set; }

        protected ControllerManager Manager { get; private set; }

        internal void SetResponse(ControllerManager manager, Response response)
        {
            Response = response;
            Manager = manager;
        }

        protected void ContentFor(string yieldIdentifier, ResponseHandler handler)
        {
            Response.ContentFor(yieldIdentifier, handler);
        }

        protected void Layout(string fileName)
        {
            _layout = GetHandler(fileName);
        }

        protected void Render(string fileName)
        {
            var handler = GetHandler(fileName);
            ContentFor("", handler);
            Response.Render(_layout);
        }

        protected ResponseHandler GetHandler(string fileName)
        {
            var handler = Manager.GetFileHandler(fileName);
            if (handler == null)
            {
                throw new KeyNotFoundException("Handler for file: " + fileName);
            }

            return handler;
        }
    }
}
