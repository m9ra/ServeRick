using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Defaults
{
    public class ResponseManager : ResponseManagerBase
    {
        public ResponseManager(WebApplication app, string rootPath, params Type[] controllers)
            : base(app, rootPath, controllers)
        {
            ErrorPage(404, "404.haml");
            PublicExtensions("png", "gif", "jpg", "css", "js", "scss", "md", "swf", "ico", "txt");
        }
    }
}
