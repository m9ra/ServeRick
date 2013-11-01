using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;
using ServeRick.Database;
using ServeRick.Modules;

namespace TestWebApp
{
    class TestWeb : WebApplication
    {
        readonly string _rootPath;

        internal TestWeb(string rootPath)
        {
            _rootPath = rootPath + "/";
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
            /**/
            manager.AddDirectoryContent("");
            /*/       manager.AddFileResource("test.haml");/**/

            return manager;
        }

        protected override InputManagerBase createInputManager()
        {
            return new InputManager();
        }

        protected override IEnumerable<DataTable> createTables()
        {
            var driver = new LightDataDriver(new []{
                    new TestItem(1,"Data1"),
                    new TestItem(2,"Data2")
                });

            return new DataTable[]{
                new DataTable<TestItem>(driver)
            };
        }
    }
}
