﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;
using System.Linq.Expressions;

using ServeRick.Networking;
using ServeRick.Responsing;
using ServeRick.Compiling;

namespace ServeRick
{
    public abstract class ResponseManagerBase
    {
        /// <summary>
        /// Root for resource loading
        /// </summary>
        public readonly string RootPath;

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
        /// Extensions that are published directly via URI
        /// </summary>
        readonly HashSet<string> _publicExtensions = new HashSet<string>();

        /// <summary>
        /// File for missing file action
        /// </summary>
        protected WebItem _404;

        /// <summary>
        /// Web item for not modified response
        /// </summary>
        protected WebItem _304;

        /// <summary>
        /// Owning web application
        /// </summary>
        protected readonly WebApplication Application;

        /// <summary>
        /// Handler provider available for current controller
        /// </summary>
        protected ResponseHandlerProvider HandlerProvider { get { return Application.HandlerProvider; } }

        /// <summary>
        /// Processor which allows to change uri before action dispatch.
        /// </summary>
        protected virtual string uriProcessor(string originalUri, Client client) { return originalUri; }

        public ResponseManagerBase(WebApplication application, string rootPath, params Type[] controllers)
        {
            RootPath = rootPath + Path.DirectorySeparatorChar;
            Application = application;
            foreach (var controller in controllers)
            {
                foreach (var action in controller.GetMethods())
                {
                    if (action.DeclaringType.IsSubclassOf(typeof(ResponseController)))
                        registerAction(action);
                }
            }

            _304 = WebItem.Runtime(_handler_304);
        }

        #region Resource loading routines

        public void AddDirectoryContent(string relativeDirPath)
        {
            //TODO hook directory for listening changes
            relativeDirPath += Path.DirectorySeparatorChar;
            foreach (var file in Directory.EnumerateFiles(RootPath + relativeDirPath))
            {
                var relativeFilePath = relativeDirPath + Path.GetFileName(file);
                AddFileResource(relativeFilePath);
            }
        }

        public void AddDirectoryTree(string relativeDirPath)
        {
            AddDirectoryContent(relativeDirPath);

            foreach (var subDir in Directory.EnumerateDirectories(RootPath + relativeDirPath))
            {
                var relativeSubDirPath = relativeDirPath + Path.DirectorySeparatorChar + Path.GetFileName(subDir);
                AddDirectoryTree(relativeSubDirPath);
            }
        }

        public void AddFileResource(string relativePath)
        {
            var handler = getWebItem(relativePath);

            RegisterFile(getId(relativePath), handler);

            if (isPublic(relativePath))
            {
                var uri = getUri(relativePath);
                PublishAction(uri, handler);
            }
        }

        protected string getId(string relativePath)
        {
            relativePath = relativePath.Replace("\\", "/");
            if (relativePath.StartsWith("/"))
                relativePath = relativePath.Substring(1);

            return relativePath;
        }


        protected virtual string getUri(string relativeFilePath)
        {
            var uri = relativeFilePath.Replace('\\', '/');
            if (uri[0] != '/')
                uri = '/' + uri;

            return uri;
        }

        private bool isPublic(string relativeFilePath)
        {
            var ext = getExtension(relativeFilePath);
            return _publicExtensions.Contains(ext);
        }

        private string getExtension(string path)
        {
            var extWithDot = Path.GetExtension(path);
            return extWithDot.StartsWith(".") ? extWithDot.Substring(1) : extWithDot;
        }

        private WebItem getWebItem(string relativeFilePath)
        {
            var file = RootPath + relativeFilePath;

            Console.WriteLine("Processing file: {0}", file);

            var ext = getExtension(file);

            switch (ext.ToLower())
            {
                case "haml":
                    return CompileHAML(file);
                case "scss":
                    return CompileSCSS(file);
                case "gif":
                case "css":
                case "png":
                case "jpg":
                case "bmp":
                case "txt":
                case "js":
                case "md":
                case "swf":
                case "html":
                case "htm":
                case "ico":
                case "svg":
                case "eot":
                case "ttf":
                case "woff":
                case "woff2":
                case "wasm":
                case "mem":
                case "":
                    return SendRaw(file, ext);
                default:
                    throw new NotSupportedException("Unsupported format");
            }
        }

        #endregion

        #region Controller manager services

        protected void PublicExtensions(params string[] extensions)
        {
            _publicExtensions.UnionWith(extensions);
        }

        protected void ErrorPage(int errorCode, string relativeFilePath)
        {
            switch (errorCode)
            {
                case 404:
                    _404 = getWebItem(relativeFilePath);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determine whether there is an action regeistered with given path.
        /// </summary>
        protected bool HasActionFor(string relativeFilePath)
        {
            return _actions.ContainsKey(relativeFilePath);
        }

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
                var contentType = getCompilationMime(language);
                if (isCacheable(language))
                {
                    item.ETag = DateTime.Now.Ticks.ToString();
                }

                //TODO resolve source format
                var handler = HandlerProvider.Compile(language, source);
                if (handler == null)
                {
                    throw new NotSupportedException("Compilation failed: " + language + " in " + file);
                }
                item.Handler = (r) =>
                {
                    try
                    {
                        r.SetContentType(contentType);
                        handler(r);
                    }
                    catch (Exception)
                    {
                        Log.Error("Request handling failed in file {0}: {1}", file, r.Client.Request.RequestURI);
                        throw;
                    }
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
                item.ETag = DateTime.Now.Ticks.ToString();

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

        private void _handler_304(Response response)
        {
            response.SetStatus(304);
            response.SetETag(response.Client.Request.ExpectedETag);
        }

        /// <summary>
        /// Get handler for file
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>Handler for file</returns>
        internal ResponseHandler GetFileHandler(string fileName)
        {
            WebItem item;
            if (!_files.TryGetValue(fileName, out item))
                Log.Error("File handler for file {0} is not available.", fileName);
            return item.Handler;
        }

        /// <summary>
        /// Handle client with parsed request
        /// </summary>
        /// <param name="client">Client to be handled</param>
        internal void Handle(Client client)
        {
            var uri = client.Request.URI;
            uri = uriProcessor(uri, client);

            WebItem item;
            if (!_actions.TryGetValue(uri, out item))
            {
                item = _404;
                client.Response.SetStatus(404);
            }

            if (item.ETag != null)
            {
                if (client.Request.ExpectedETag == item.ETag)
                {
                    item = _304;
                }
                else
                {
                    client.Response.SetETag(item.ETag);
                }
            }

            client.EnqueueWork(
                new ResponseHandlerWorkItem(client, item.Handler)
                );

            client.StartChainProcessing();
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Determine whether give file is locked.
        /// </summary>
        /// <param name="file">File to read.</param>
        /// <returns>File content if available, null otherwise.</returns>
        private string tryReadFile(string file)
        {
            try
            {
                return File.ReadAllText(file);
            }
            catch (IOException)
            {
                return null;
            }
        }

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
                item.Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                item.Watcher.Created += (s, e) => itemChangeHandler(action);
                item.Watcher.Renamed += (s, e) => itemChangeHandler(action);
                item.Watcher.Changed += (s, e) => itemChangeHandler(action);
            }
        }

        private void itemChangeHandler(Action action)
        {
            lock (_L_itemRefresh)
            {
                try
                {
                    action();
                    ResponseController.ClearCaches();

                }
                catch (Exception)
                {
                    Log.Notice("Item refresh action caused exception");
                }
            }
        }

        private string getSource(string file)
        {
            try
            {
                for (var i = 0; i < 5; ++i)
                {
                    var content = tryReadFile(file);
                    if (content != null)
                        return content;

                    System.Threading.Thread.Sleep(500);
                }

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
            var actionConstant = Expression.Constant(this);

            var controller = Expression.New(action.DeclaringType);
            var controllerVar = Expression.Variable(controller.Type);
            var responseControllerType = typeof(ResponseController);
            var methodFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            var actionMethod = Expression.Block(new[] { controllerVar },
                Expression.Assign(controllerVar, controller),
                Expression.Call(controllerVar, responseControllerType.GetMethod("SetResponse", methodFlags), managerConstant, responseParam),
                Expression.Call(controllerVar, action)
            );

            var handler = Expression.Lambda<ResponseHandler>(actionMethod, responseParam).Compile();

            var item = WebItem.Runtime(handler);
            var name = action.Name.Replace("__", ".");

            return new ActionInfo("/" + name, item);
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
                case "eot":
                    return "image/eot";
                case "ttf":
                    return "image/ttf";
                case "woff":
                    return "image/woff";
                case "woff2":
                    return "image/woff2";
                case "jpg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "svg":
                    return "image/svg";
                case "gif":
                    return "image/gif";
                case "bmp":
                    return "image/bmp";
                case "ico":
                    return "image/x-icon";
                case "":
                case "md":
                case "txt":
                    return "text/plain";
                case "htm":
                case "html":
                    return "text/html";
                case "mem":
                case "wasm":
                case "js":
                    return "application/javascript";
                case "swf":
                    return "application/x-shockwave-flash";
                default:
                    throw new NotImplementedException();
            }
        }

        private bool isCacheable(string language)
        {
            switch (language)
            {
                case "scss":
                case "css":
                case "js":
                    return true;

                default:
                    return false;
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
