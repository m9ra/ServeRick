using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;

namespace TestWebApp
{
    class WebHelper : Helper
    {
        public static string link_to(string path,string text)
        {
            return "<a href=\"" + path+"\">"+text+"</a>";
        }

        public static string image_tag(string file)
        {
            return "<img src=\"" + file + "\"/>";
        }

        public static string print(string text)
        {
            return text;
        }
    }
}
