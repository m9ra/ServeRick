using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Networking;
using SharpServer.Compiling;

using System.IO;

using SharpServer;

namespace TestWebApp
{
    /// <summary>
    /// Manage controllers and set handlers for client requests
    /// </summary>
    class SimpleControllerManager : ControllerManager
    {
        readonly string _rootPath;

        private readonly string[] _publicExtensions = new[]{
            "jpg","css","scss"
        };

        internal SimpleControllerManager(WebApplication app, string rootPath)
            : base(app,
                typeof(SimpleController)
            )
        {
            _rootPath = rootPath;            
            _404 = getWebItem("404.haml");            
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
            var handler = getWebItem(fileRelative);

            RegisterFile(fileRelative, handler);

            if (isPublic(fileRelative))
            {
                var uri = getUri(fileRelative);
                PublishAction(uri, handler);
            }
        }

        private string getUri(string filePath)
        {
            return "/" + filePath;
        }

        private bool isPublic(string fileRelative)
        {
            var ext = Path.GetExtension(fileRelative).Substring(1);
            return _publicExtensions.Contains(ext);
        }

        private WebItem getWebItem(string fileRelative)
        {
            var file = _rootPath + fileRelative;
            var ext = Path.GetExtension(file).Substring(1);

            switch (ext.ToLower())
            {
                case "haml":
                    return CompileHAML(file);
                case "scss":
                    return CompileSCSS(file);
                case "css":
                case "jpg":
                case "txt":
                    return  SendRaw(file, ext);
                default:
                    throw new NotImplementedException();
            }
        } 
    }
}
