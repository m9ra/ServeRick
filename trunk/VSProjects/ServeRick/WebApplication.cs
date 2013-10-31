using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Database;
using ServeRick.Compiling;

namespace ServeRick
{
    public abstract class WebApplication
    {
        public readonly ResponseHandlerProvider HandlerProvider;

        protected abstract ResponseManagerBase createResponseManager();

        protected abstract InputManagerBase createInputManager();

        protected abstract IEnumerable<DataTable> createTables();

        protected abstract Type[] getHelpers();

        protected WebApplication()
        {
            var helperTypes=getHelpers();
            var webHelpers=new WebMethods(helperTypes);
            HandlerProvider = new ResponseHandlerProvider(webHelpers);
        }
        
        internal ResponseManagerBase CreateResponseManager(){
            var manager=createResponseManager();
            return manager;
        }

        internal InputManagerBase CreateInputManager()
        {
            var manager = createInputManager();
            return manager;
        }

        internal IEnumerable<DataTable> CreateTables()
        {
            var tables = createTables();
            return tables;
        }
    }
}
