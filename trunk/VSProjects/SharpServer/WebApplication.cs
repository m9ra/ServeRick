using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Compiling;

namespace SharpServer
{
    public abstract class WebApplication
    {
        public readonly ResponseHandlerProvider HandlerProvider;

        protected abstract ControllerManager createManager();

        protected abstract Type[] getHelpers();

        protected WebApplication()
        {
            var helperTypes=getHelpers();
            var webHelpers=new WebMethods(helperTypes);
            HandlerProvider = new ResponseHandlerProvider(webHelpers);
        }
        
        internal ControllerManager CreateManager(){
            var manager=createManager();

            return manager;
        }
    }
}
