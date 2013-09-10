using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer
{
    static class NarioShop
    {
        internal static ControllerManager GetManager()
        {
            var manager=new SimpleControllerManager("www/");

            manager.AddAll();
            manager.SetRoot("index.haml");

            return manager;
        }
    }
}
