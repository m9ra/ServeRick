using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using SharpServer.Networking;

using SharpServer.Compiling;

namespace SharpServer
{
    public abstract class ControllerManager
    {
        /// <summary>
        /// Response handlers for uri requests
        /// TODO: pattern based resolving of response handlers
        /// </summary>
        readonly Dictionary<string, ResponseHandler> _actions = new Dictionary<string, ResponseHandler>();

        /// <summary>
        /// Response handlers for file requests
        /// </summary>
        readonly Dictionary<string, ResponseHandler> _files = new Dictionary<string, ResponseHandler>();

        /// <summary>
        /// Response handler for missing action
        /// </summary>
        protected ResponseHandler _404;

        public ControllerManager(params Type[] controllers)
        {
            foreach (var controller in controllers)
            {
                foreach (var action in controller.GetMethods())
                {
                    if (action.DeclaringType.IsSubclassOf(typeof(Controller)))
                        registerAction(action);
                }
            }
        }

        /// <summary>
        /// Get handler for file
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>Handler for file</returns>
        internal ResponseHandler GetFileHandler(string fileName)
        {
            ResponseHandler handler;
            _files.TryGetValue(fileName, out handler);
            return handler;
        }

        /// <summary>
        /// Handle client with parsed request
        /// </summary>
        /// <param name="client">Client to be handled</param>
        internal void Handle(Client client)
        {
            var uri = client.Request.URI;

            ResponseHandler handler;
            if (!_actions.TryGetValue(uri, out handler))
            {
                handler = _404;
            }

            client.Response.EnqueueToProcessor(handler);
        }
        
        /// <summary>
        /// Register handler for given uri
        /// </summary>
        /// <param name="file">Uri to register</param>
        /// <param name="handler">Handler for given uri</param>
        protected void RegisterFileHandler(string file, ResponseHandler handler)
        {
            _files.Add(file, handler);
        }

        protected void RegisterActionHandler(string uri, ResponseHandler handler)
        {
            _actions.Add(uri, handler);
        }

        private void registerAction(MethodInfo action)
        {
            var actionInfo = resolveAction(action);
            addAction(actionInfo.Pattern, actionInfo.Handler);
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

            return new ActionInfo("/" + action.Name, handler);
        }

        private void addAction(string pattern, ResponseHandler handler)
        {
            if (pattern == "/index")
            {
                _actions.Add("/", handler);
            }

            _actions.Add(pattern, handler);
        }
    }
}
