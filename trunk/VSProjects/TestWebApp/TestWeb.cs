using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer;

namespace TestWebApp
{
    class TestWeb:WebApplication
    {
        public override ControllerManager CreateManager()
        {
            var manager=new SimpleControllerManager("www/");

            manager.AddAll();
            manager.SetRoot("index.haml");
            
            return manager;
        }
    }
}
