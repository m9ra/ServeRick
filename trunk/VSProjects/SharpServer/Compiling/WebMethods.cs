using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace SharpServer.Compiling
{
    public class WebMethods
    {
        Dictionary<string, WebMethod> _methods = new Dictionary<string, WebMethod>();

        readonly Type[] _helperClasses;

        public WebMethods(params Type[] helperClasses)
        {
            _helperClasses = helperClasses;

            foreach (var helperClass in helperClasses)
            {
                foreach (var method in helperClass.GetMethods())
                {
                    registerMethod(method);
                }
            }
        }

        internal WebMethod GetMethod(string methodName)
        {
            return _methods[methodName];
        }

        private void registerMethod(MethodInfo info)
        {
            var method = new WebMethod(info);

            _methods[method.Name] = method;
        }
    }
}
