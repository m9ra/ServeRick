using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Networking;
using ServeRick.Compiling;

using System.IO;

using ServeRick;

namespace TestWebApp
{
    /// <summary>
    /// Manage controllers and set handlers for client requests
    /// </summary>
    class SimpleControllerManager : ResponseManagerBase
    {

        internal SimpleControllerManager(WebApplication app, string rootPath)
            : base(app,rootPath,
                typeof(SimpleController)
            )
        {
            ErrorPage(404, "404.haml");
            PublicExtensions("png", "jpg", "css", "scss");
        }
    }
}
