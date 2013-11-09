using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Networking;
using ServeRick.Sessions;

namespace ServeRick
{
    public abstract class InputController
    {
        /// <summary>
        /// Client handled by current input controller
        /// </summary>
        internal Client Client;

        /// <summary>
        /// Request which input is controlled by current controller
        /// </summary>
        protected HttpRequest Request { get { return Client.Parser.Request; } }

        /// <summary>
        /// Accept incomming data
        /// </summary>
        /// <param name="data">buffer where data are stored</param>
        /// <param name="dataOffset">offset where incoming data are stored</param>
        /// <param name="dataLength">length of incomming data</param>
        protected abstract void acceptData(byte[] data, int dataOffset, int dataLength);

        /// <summary>
        /// Determine that data downloading still continues.
        /// </summary>
        public bool ContinueDownloading { get; protected set; }

        /// <summary>
        /// Unique ID of input (uniquness accross all input controllers created 
        /// via input manager)
        /// </summary>
        public long InputUID { get; internal set; }

        protected InputController()
        {
            ContinueDownloading = true;
        }

        /// <summary>
        /// Accept incomming data
        /// </summary>
        /// <param name="data">buffer where data are stored</param>
        /// <param name="dataOffset">offset where incoming data are stored</param>
        /// <param name="dataLength">length of incomming data</param>
        internal void AcceptData(byte[] data, int dataOffset, int dataLength)
        {
            acceptData(data, dataOffset, dataLength);
        }

        /// <summary>
        /// Set session of current client. Session value can be
        /// retrieved even before input controller handles input.
        /// NOTE: It is usefull for geting info about upload processing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionData"></param>
        public void SetSession<T>(T sessionData)
        {
            var work = new SetSessionWorkItem(Client.Unit, Client.SessionID, sessionData);
            Client.Unit.Output.EnqueueWork(work);
        }

        /// <summary>
        /// Set POST variable to given value
        /// </summary>
        /// <param name="varName">Name of post variable</param>
        /// <param name="varValue">Value of post variable</param>
        public void SetPOST(string varName, string varValue)
        {
            Request.SetPOST(varName, varValue);
        }
    }
}
