using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;

namespace TestWebApp
{
    class SimpleController : ResponseController
    {
        public void index()
        {
            Query<TestItem>().Find(2).ExecuteRow((row) =>
            {
                SetParam("row", row);

                Layout("application.haml");
                Render("index.haml");
            });
        }

        [GET("/attribs/:name/:page")]
        public void nabizime(/*string name, int page*/)
        {
            Layout("application.haml");
            Render("index.haml");
        }

        public void test()
        {
            SetParam("test", GET("test"));
            SetParam("test2", new[] { "ValA", "ValB" });
            Layout("application.haml");
            //Layout("test.haml");
            Render("test.haml");
        }
    }
}
