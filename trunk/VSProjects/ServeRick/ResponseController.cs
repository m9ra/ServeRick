using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using ServeRick.Sessions;
using ServeRick.Database;
using ServeRick.Networking;
using ServeRick.Processing;

namespace ServeRick
{
    public abstract class ResponseController
    {
        private ResponseHandler _layout = null;

        private Client Client { get { return Response.Client; } }

        private ProcessingUnit Unit { get { return Client.Unit; } }

        protected Response Response { get; private set; }

        protected HttpRequest Request { get { return Client.Request; } }

        protected ResponseManagerBase Manager { get; private set; }

        protected SelectQuery<ActiveRecord> Query<ActiveRecord>()
            where ActiveRecord : DataRecord
        {
            return new SelectQuery<ActiveRecord>(Response);
        }

        protected InsertQuery<ActiveRecord> Insert<ActiveRecord>(IEnumerable<ActiveRecord> entries)
            where ActiveRecord : DataRecord
        {
            return new InsertQuery<ActiveRecord>(Response, entries);
        }

        internal void SetResponse(ResponseManagerBase manager, Response response)
        {
            Response = response;
            Manager = manager;
            Response.AllowSessionFlip();
        }

        protected void ContentFor(string yieldIdentifier, ResponseHandler handler)
        {
            Response.ContentFor(yieldIdentifier, handler);
        }

        protected void ContentFor(string yieldIdentifier, string partial)
        {
            var partialHandler = GetHandler(partial);
            Response.ContentFor(yieldIdentifier, partialHandler);
        }

        protected void Layout(string fileName)
        {
            _layout = GetHandler(fileName);
        }

        protected void Render(string fileName)
        {
            var handler = GetHandler(fileName);
            if (_layout == null)
            {
                Response.Render(handler);
            }
            else
            {
                ContentFor("", handler);
                Response.Render(_layout);
            }
        }

        protected string GET(string varName)
        {
            return Request.GetGET(varName);
        }

        protected string POST(string varName)
        {
            return Request.GetPOST(varName);
        }

        protected T Session<T>()
        {
            return SessionProvider.GetData<T>(Unit.Output, Client.SessionID);
        }

        protected void Session<T>(T data)
        {
            SessionProvider.SetData(Unit.Output, Client.SessionID, data);
        }

        protected void RemoveSession<T>()
        {
            SessionProvider.RemoveData<T>(Unit.Output, Client.SessionID);
        }

        /// <summary>
        /// Set value to given flash type. Flash is one display durability session data.
        /// </summary>
        /// <param name="messageID">ID of flash that will be set</param>
        /// <param name="value">Value of message that will be set</param>
        protected void Flash(string messageID, string value)
        {
            SessionProvider.SetFlash(Unit.Output, Client.SessionID, messageID, value);
        }

        /// <summary>
        /// Get flash value for given messageID.
        /// </summary>
        /// <param name="messageID">ID of flash message</param>
        /// <returns>Flash message if present, null otherwise</returns>
        protected string Flash(string messageID)
        {
            return SessionProvider.GetFlash(Unit.Output, Client.SessionID, messageID);
        }


        protected void RedirectTo(string url)
        {
            Response.SetHeader("Location", url);
            Response.SetStatus(302);
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
