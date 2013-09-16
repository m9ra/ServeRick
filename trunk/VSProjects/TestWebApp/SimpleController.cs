using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer;

namespace TestWebApp
{
    class SimpleController : Controller
    {
        public void index()
        {
            Render("index.haml");
        }

        [GET("/attribs/:name/:page")]
        public void attribs(/*string name, int page*/)
        {
            Render("attribs.haml");
        }
    }
}
