using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.Reflection;

using ServeRick.Sessions;
using ServeRick.Database;
using ServeRick.Networking;
using ServeRick.Processing;
using ServeRick.Responsing;

namespace ServeRick
{
    public abstract class ResponseController
    {
        public static readonly int TimepointCount = 8;

        public string RootPath { get { return Manager.RootPath; } }

        private static int[] _counts = new int[TimepointCount];

        private static int[] _miliseconds = new int[TimepointCount];

        private ResponseHandler _layout = null;

        private Client Client { get { return Response.Client; } }

        private ProcessingUnit Unit { get { return Client.Unit; } }

        protected HttpRequest Request { get { return Client.Request; } }

        protected ResponseManagerBase Manager { get; private set; }

        public Response Response { get; private set; }

        protected static SelectQuery<ActiveRecord> Query<ActiveRecord>()
            where ActiveRecord : DataRecord
        {
            return new SelectQuery<ActiveRecord>();
        }

        protected InsertQuery<ActiveRecord> Insert<ActiveRecord>(IEnumerable<ActiveRecord> entries)
            where ActiveRecord : DataRecord
        {
            return new InsertQuery<ActiveRecord>(entries);
        }

        internal void SetResponse(ResponseManagerBase manager, Response response)
        {
            Response = response;
            Manager = manager;
            Response.AllowSessionFlip();
        }

        protected void TimePoint(int id)
        {
            var fromStart = Client.TimeFromStart;
            Interlocked.Add(ref _miliseconds[id], fromStart);
            Interlocked.Increment(ref _counts[id]);
        }

        public static void ResetTimepoints()
        {
            _miliseconds = new int[TimepointCount];
            _counts = new int[TimepointCount];
        }

        public static int AverageMilliseconds(int id)
        {
            var sum = _miliseconds[id];
            var count = _counts[id];

            if (count == 0)
                return 0;

            return sum / count;
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

        protected void Write(DataStream data)
        {
            Client.EnqueueWork(new WriteWorkItem(Client, data));
        }

        /// <summary>
        /// Handle headers of partial request and response.
        /// If request is not a partial, set appropriate content length.
        /// </summary>
        /// <param name="totalLength">Length of full response</param>
        /// <returns>Start offset for response</returns>
        protected int HandlePartial(int totalLength, out int requestedLength)
        {
            string rangeValue;
            requestedLength = totalLength;
            if (Request.TryGetHeader("Range", out rangeValue))
            {
                var rangePrefix = "bytes=";

                var ranges = rangeValue.Split('-');
                if (ranges.Length != 2 || !ranges[0].StartsWith(rangePrefix))
                {
                    //wrong range format
                    Log.Error("Range request has wrong format {0}", rangeValue);
                    return 0;
                }

                var from = getRangeValue(ranges[0].Substring(rangePrefix.Length), 0);
                var to = getRangeValue(ranges[1], totalLength - 1);

                if (from > to)
                {
                    Log.Error("Unsupported range specified by {0}", rangeValue);
                    return 0;
                }

                var responseRange = "bytes " + from + "-" + to + "/" + totalLength;
                requestedLength = to - from + 1;

                Response.SetStatus(206);
                Response.SetHeader("Content-Range", responseRange);
                Response.SetLength(requestedLength);

                return from;
            }
            else
            {
                //there is no range specified
                Response.SetLength(totalLength);
                return 0;
            }
        }

        private int getRangeValue(string range, int borderValue)
        {
            if (range == "")
                return borderValue;

            int result;
            int.TryParse(range, out result);

            return result;
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

        protected T Session<T>(T data)
        {
            SessionProvider.SetData(Unit.Output, Client.SessionID, data);
            return data;
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

        #region Database API

        protected void Execute<T>(UpdateQuery<T> query, UpdateExecutor<T> executor = null)
            where T : DataRecord
        {
            var item = query.CreateWork(executor);
            Client.EnqueueWork(item);
        }

        protected void Execute<T>(InsertQuery<T> query, InsertExecutor<T> executor)
            where T : DataRecord
        {
            var item = query.CreateWork(executor);

            Client.EnqueueWork(item);
        }

        protected void ExecuteRow<T>(SelectQuery<T> query, RowExecutor<T> executor)
            where T : DataRecord
        {
            var item = query.CreateWork(executor);

            Client.EnqueueWork(item);
        }

        protected void ExecuteRows<T>(SelectQuery<T> query, RowsExecutor<T> executor)
            where T : DataRecord
        {
            var item = query.CreateWork(executor);

            Client.EnqueueWork(item);
        }

        #endregion
    }
}
