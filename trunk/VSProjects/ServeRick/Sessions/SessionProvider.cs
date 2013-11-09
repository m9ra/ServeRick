using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Networking;

namespace ServeRick.Sessions
{
    class SessionProvider
    {
        internal static readonly string SessionCookie = "ServeRick_SESSID";

        /// <summary>
        /// Read associate, generate session id for client
        /// </summary>
        /// <param name="client"></param>
        internal void Handle(Client client)
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
    }
}
