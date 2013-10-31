using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using ServeRick.Database;
using ServeRick.Networking;

namespace ServeRick
{
    public abstract class ResponseController
    {
        private ResponseHandler _layout = null;

        protected Response Response { get; private set; }

        protected HttpRequest Request { get { return Response.Client.Request; } }

        protected ResponseManagerBase Manager { get; private set; }

        protected TableQuery<ActiveRecord> Query<ActiveRecord>()
            where ActiveRecord : DataRecord
        {
            return new TableQuery<ActiveRecord>(Response);
        }

        internal void SetResponse(ResponseManagerBase manager, Response response)
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

        protected string GET(string varName)
        {
            return Request.GetGET(varName);
        }

        protected string POST(string varName)
        {
            return Request.GetPOST(varName);
        }

        protected void SetParam(string paramName, object paramValue)
        {
            Response.SetParam(paramName, paramValue);
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
