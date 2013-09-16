using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer
{
    public abstract class WebApplication
    {
        protected abstract ControllerManager createManager();

        internal ControllerManager CreateManager(){
            var manager=createManager();

            return manager;
        }
    }
}
