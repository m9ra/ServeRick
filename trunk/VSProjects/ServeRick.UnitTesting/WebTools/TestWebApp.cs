using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.UnitTesting.WebTools
{
    class TestWebApp:WebApplication
    {

        protected override Type[] getHelpers()
        {
            return new Type[0];
        }

        protected override ResponseManagerBase createResponseManager()
        {
            throw new NotImplementedException();
        }

        protected override InputManagerBase createInputManager()
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<Database.DataTable> createTables()
        {
            throw new NotImplementedException();
        }
    }
}
