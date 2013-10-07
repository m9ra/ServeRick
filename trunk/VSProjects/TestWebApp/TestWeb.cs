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

        protected override ControllerManager createManager()
        {
            var manager = new SimpleControllerManager(this, _rootPath);
            manager.AddAll();

            return manager;
        }

        protected override Type[] getHelpers()
        {
            return new[]{
                typeof(WebHelper)
            };
        }
    }
}
