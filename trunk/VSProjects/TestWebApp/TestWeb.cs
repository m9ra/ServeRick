using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer;

namespace TestWebApp
{
    class TestWeb : WebApplication
    {
        readonly string _rootPath;

        internal TestWeb(string rootPath)
        {
            _rootPath = rootPath+"/";
        }
        
        protected override Type[] getHelpers()
        {
            return new[]{
                typeof(WebHelper)
            };
        }

        protected override ResponseManagerBase createResponseManager()
        {
            var manager = new SimpleControllerManager(this, _rootPath);
            manager.AddDirectoryContent("");

            return manager;
        }

        protected override InputManagerBase createInputManager()
        {
            return new InputManager();
        }
    }
}
