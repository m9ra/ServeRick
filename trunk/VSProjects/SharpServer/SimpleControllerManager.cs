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
    class SimpleControllerManager:ControllerManager
    {
        readonly string _rootPath;

        readonly Dictionary<string, ResponseHandler> _files = new Dictionary<string, ResponseHandler>();

        readonly ResponseHandler _404;

        internal SimpleControllerManager(string rootPath)
        {
            _rootPath = rootPath;
            _404 = getHandler("404.haml");
        }

        internal void SetRoot(string fileRelative)
        {
            var uri = getUri(fileRelative);

            _files["/"] = _files[uri];
        }

        internal void AddAll()
        {
            foreach (var file in Directory.EnumerateFiles(_rootPath))
            {
                var fileName = Path.GetFileName(file);
                AddPath(fileName);
            }
        }

        internal void AddPath(string fileRelative)
        {
            var uri = getUri(fileRelative);
            _files[uri] = getHandler(fileRelative);
        }

        internal override void Handle(Client client)
        {
            var uri=client.Parser.Request.URI;

            ResponseHandler handler;
            if (!_files.TryGetValue(uri,out handler))
            {
                handler = _404;   
            }
                        
            client.Response.Render(handler);
        }

        private string getUri(string filePath)
        {
            return "/" + filePath;
        }

        private ResponseHandler getHandler(string fileRelative)
        {
            var file = _rootPath + fileRelative;
            var ext = Path.GetExtension(file).Substring(1);

            switch (ext.ToLower())
            {
                case "haml":
                    return compileHAML(file);
                case "jpg":
                case "txt":
                    return sendRaw(file,ext);
                default:
                    throw new NotImplementedException();
            }
            
        }

        private ResponseHandler sendRaw(string file,string ext)
        {
            var bytes = File.ReadAllBytes(file);
            var mime = getMime(ext);

            return (r) =>
            {
                r.SetContentType(mime);
                r.SetLength(bytes.Length);
                r.Write(bytes);
            };
        }

        private string getMime(string ext){
            switch(ext){
                case "jpg":
                    return "image/jpeg";
                case "txt":
                    return "text/plain";
                default:
                    throw new NotImplementedException();
            }
        }

        private ResponseHandler compileHAML(string file)
        {
            var source = getSource(file);
            var handler = ResponseHandlerProvider.GetHandler("haml", source);
            if (handler == null)
            {
                throw new NotSupportedException("Compilation failed");
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
