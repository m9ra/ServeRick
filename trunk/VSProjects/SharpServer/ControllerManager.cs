using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;
using System.Linq.Expressions;

using SharpServer.Networking;

using SharpServer.Compiling;

namespace SharpServer
{
    public abstract class ControllerManager
    {
        object _L_itemRefresh = new object();

        /// <summary>
        /// Response handlers for uri requests
        /// TODO: pattern based resolving of response handlers
        /// </summary>
        readonly Dictionary<string, WebItem> _actions = new Dictionary<string, WebItem>();

        /// <summary>
        /// Response handlers for file requests
        /// </summary>
        readonly Dictionary<string, WebItem> _files = new Dictionary<string, WebItem>();

        /// <summary>
        /// File for missing file action
        /// </summary>
        protected WebItem _404;

        /// <summary>
        /// Owning web application
        /// </summary>
        protected readonly WebApplication Application;

        /// <summary>
        /// Handler provider available for current controller
        /// </summary>
        protected ResponseHandlerProvider HandlerProvider { get { return Application.HandlerProvider; } }

        public ControllerManager(WebApplication application, params Type[] controllers)
        {
            Application = application;
            foreach (var controller in controllers)
            {
                foreach (var action in controller.GetMethods())
                {
                    if (action.DeclaringType.IsSubclassOf(typeof(Controller)))
                        registerAction(action);
                }
            }
        }

        #region Controller manager services

        /// <summary>
        /// Publish file item with given uri
        /// </summary>
        /// <param name="fileId">Id of registered file</param>
        /// <param name="fileItem">Published file item</param>
        protected void RegisterFile(string fileId, WebItem fileItem)
        {
            _files.Add(fileId, fileItem);
        }

        /// <summary>
        /// Publish action item with given uri
        /// </summary>
        /// <param name="uri">Uri to publish</param>
        /// <param name="actionItem">Published action item</param>
        protected void PublishAction(string uri, WebItem actionItem)
        {
            _actions.Add(uri, actionItem);
        }

        /// <summary>
        /// Compile given file into web item
        /// </summary>
        /// <param name="file">File to be compiled</param>
        /// <returns>Compiled file</returns>
        protected WebItem CompileHAML(string file)
        {
            return Compile(file, "haml");
        }

        /// <summary>
        /// Compile given file into web item
        /// </summary>
        /// <param name="file">File to be compiled</param>
        /// <returns>Compiled file</returns>
        protected WebItem CompileSCSS(string file)
        {
            return Compile(file, "scss");
        }

        protected WebItem Compile(string file, string language)
        {
            var item = new WebItem(file);            
            fillItem(item, () =>
            {
                var source = getSource(item.FilePath);
                var contentType=getCompilationMime(language);

                //TODO resolve source format
                var handler = HandlerProvider.Compile(language, source);
                if (handler == null)
                {
                    throw new NotSupportedException("Compilation failed");
                }
                item.Handler = (r) =>
                {
                    r.SetContentType(contentType);
                    handler(r);
                };
            });
            return item;
        }


        protected WebItem SendRaw(string file, string ext)
        {
            var item = new WebItem(file);

            fillItem(item, () =>
            {
                var bytes = File.ReadAllBytes(file);
                var mime = getMime(ext);


                item.Handler = (r) =>
                {
                    r.SetContentType(mime);
                    r.SetLength(bytes.Length);
                    r.Write(bytes);
                };
            });

            return item;
        }

        #endregion

        #region Internal methods for response handling

        /// <summary>
        /// Get handler for file
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>Handler for file</returns>
        internal ResponseHandler GetFileHandler(string fileName)
        {
            WebItem item;
            _files.TryGetValue(fileName, out item);
            return item.Handler;
        }

        /// <summary>
        /// Handle client with parsed request
        /// </summary>
        /// <param name="client">Client to be handled</param>
        internal void Handle(Client client)
        {
            var uri = client.Request.URI;

            WebItem item;
            if (!_actions.TryGetValue(uri, out item))
            {
                item = _404;
            }

            client.Response.EnqueueToProcessor(item.Handler);
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Fill item with item.FilePath, is called on every file change
        /// </summary>
        /// <param name="item">Filled item</param>
        /// <param name="action">Action called when item has to be refreshed</param>
        private void fillItem(WebItem item, Action action)
        {
            action();
            if (item.Watcher == null)
            {
                var directory = Path.GetDirectoryName(item.FilePath) + Path.DirectorySeparatorChar;
                var fileName = Path.GetFileName(item.FilePath);
                item.Watcher = new FileSystemWatcher(directory, fileName);

                item.Watcher.EnableRaisingEvents = true;
                item.Watcher.NotifyFilter = NotifyFilters.LastWrite;
                item.Watcher.Changed += (s, e) =>
                {
                    lock (_L_itemRefresh)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception)
                        {
                            Log.Notice("Item refresh action caused exception");
                        }

                    }
                };
            }
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

        private void registerAction(MethodInfo action)
        {
            var actionInfo = resolveAction(action);
            addAction(actionInfo.Pattern, actionInfo.Item);
        }

        private ActionInfo resolveAction(MethodInfo action)
        {
            //TODO add support for attributes
            var responseParam = Expression.Parameter(typeof(Response));
            var managerConstant = Expression.Constant(this);

            var controller = Expression.New(action.DeclaringType);
            var controllerVar = Expression.Variable(controller.Type);

            var actionMethod = Expression.Block(new[] { controllerVar },
                Expression.Assign(controllerVar, controller),
                Expression.Call(controllerVar, typeof(Controller).GetMethod("SetResponse", BindingFlags.NonPublic | BindingFlags.Instance), managerConstant, responseParam),
                Expression.Call(controllerVar, action)
            );

            var handler = Expression.Lambda<ResponseHandler>(actionMethod, responseParam).Compile();

            var item = WebItem.Runtime(handler);

            return new ActionInfo("/" + action.Name, item);
        }

        private void addAction(string pattern, WebItem item)
        {
            if (pattern == "/index")
            {
                _actions.Add("/", item);
            }

            _actions.Add(pattern, item);
        }

        private string getMime(string ext)
        {
            switch (ext)
            {
                case "css":
                    return "text/css";
                case "jpg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "txt":
                    return "text/plain";
                case "html":
                    return "text/html";
                default:
                    throw new NotImplementedException();
            }
        }


        private string getCompilationMime(string language)
        {
            switch (language)
            {
                case "scss":
                    return getMime("css");
                case "haml":
                    return getMime("html");
                default:
                    throw new NotSupportedException("Unsupported language: " + language);
            }

        }
        #endregion
    }
}
