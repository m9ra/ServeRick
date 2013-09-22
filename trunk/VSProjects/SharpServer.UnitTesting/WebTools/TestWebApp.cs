using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.UnitTesting.WebTools
{
    class TestWebApp:WebApplication
    {
        protected override ControllerManager createManager()
        {
            throw new NotImplementedException();
        }

        protected override Type[] getHelpers()
        {
            return new Type[0];
        }
    }
}
