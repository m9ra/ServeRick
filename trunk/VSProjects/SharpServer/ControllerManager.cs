using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Networking;
using SharpServer.Compiling;

using System.IO;

namespace SharpServer
{
    /// <summary>
    /// Manage controllers and set handlers for client requests
    /// </summary>
    class ControllerManager
    {
        readonly string root = "www";

        readonly Dictionary<string, ResponseHandler> _files = new Dictionary<string, ResponseHandler>();

        internal void Handle(Client client)
        {
            var uri = client.Parser.Request.URI;
            switch (uri)
            {
                case "/":
                    uri = "/index.haml";
                    break;
            }

            var handler = getHandler(uri);
            client.Response.Render(handler);
        }

        private ResponseHandler getHandler(string file)
        {
            ResponseHandler handler;
            if (!_files.TryGetValue(file, out handler))
            {
                var source = getSource(root + file);
                handler = ResponseHandlerProvider.GetHandler("haml", source);
                if (handler == null)
                {
                    throw new NotSupportedException("Compilation failed");
                }
                _files[file] = handler;
            }

            return handler;
        }

        private string getSource(string file)
        {
            try
            {
                return File.ReadAllText(file);
            }
            catch (Exception ex)
            {
                return @"
%h1 
    Exception: 
%div " + ex.ToString().Replace(Environment.NewLine, "<br>");
            }
        }
    }
}
