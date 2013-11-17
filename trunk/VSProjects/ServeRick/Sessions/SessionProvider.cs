using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Networking;
using ServeRick.Database;
using ServeRick.Responsing;

namespace ServeRick
{
    /// <summary>
    /// Session stored 
    /// </summary>
    class SessionProvider
    {
        internal static readonly string SessionCookie = "ServeRick_SESSID";

        /// <summary>
        /// TODO: Give better contract to make sessions persistant
        /// </summary>
        private readonly Dictionary<Type, object> _dataStorage = new Dictionary<Type, object>();

        private Dictionary<string, string> _flashStorage = new Dictionary<string, string>();

        private Dictionary<string, string> _flashFlip = new Dictionary<string, string>();

        #region Session manipulation routines

        internal static void SetData(OutputProcessor sessionHolder, string sessionID, object data)
        {
            SessionProvider session;
            if (!sessionHolder.Sessions.TryGetValue(sessionID, out session))
            {
                session = new SessionProvider();
                sessionHolder.Sessions[sessionID] = session;
            }

            session.setData(data);
        }

        internal static T GetData<T>(OutputProcessor sessionHolder, string sessionID)
        {
            SessionProvider session;
            if (!sessionHolder.Sessions.TryGetValue(sessionID, out session))
            {
                return default(T);
            }

            return session.getData<T>();
        }


        internal static void RemoveData<T>(OutputProcessor sessionHolder, string sessionID)
        {
            SessionProvider session;
            if (!sessionHolder.Sessions.TryGetValue(sessionID, out session))
            {
                //there is nothing to remove
                return;
            }

            //TODO remove session itself, if needed
            session._dataStorage.Remove(typeof(T));
        }

        internal static void SetFlash(OutputProcessor sessionHolder, string sessionID, string messageID, string value)
        {
            SessionProvider session;
            if (!sessionHolder.Sessions.TryGetValue(sessionID, out session))
            {
                session = new SessionProvider();
                sessionHolder.Sessions[sessionID] = session;
            }

            session._flashFlip[messageID] = value;
        }


        internal static string GetFlash(OutputProcessor sessionHolder, string sessionID, string messageID)
        {
            SessionProvider session;
            if (!sessionHolder.Sessions.TryGetValue(sessionID, out session))
            {
                //there is no session with possible flash
                return null;
            }

            string value;
            session._flashStorage.TryGetValue(messageID, out value);
            return value;
        }

        internal static void FlipFlash(OutputProcessor sessionHolder, string sessionID)
        {
            SessionProvider session;
            if (!sessionHolder.Sessions.TryGetValue(sessionID, out session))
            {
                //there is nothing to flip
                return;
            }
            
            var xchgTmp = session._flashStorage;
            session._flashStorage = session._flashFlip;
            session._flashFlip = xchgTmp;

            xchgTmp.Clear();
        }



        /// <summary>
        /// Read associate, generate session id for client
        /// </summary>
        /// <param name="client"></param>
        internal static void PrepareSessionID(Client client)
        {
            string sessionID;
            if (!client.Request.TryGetCookie(SessionCookie, out sessionID))
            {
                //TODO improve security
                sessionID = Guid.NewGuid().ToString("N");

                client.Response.SetCookie(SessionCookie, sessionID);
            }

            client.SessionID = sessionID;
        }

        #endregion

        private void setData(object data)
        {
            _dataStorage[data.GetType()] = data;
        }

        private T getData<T>()
        {
            object data;
            if (!_dataStorage.TryGetValue(typeof(T), out data))
            {
                return default(T);
            }
            return (T)data;
        }
    }
}
