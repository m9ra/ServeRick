using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Diagnostics;

using System.Net;

using ServeRick.Networking;
using ServeRick.Compiling;
using ServeRick.Memory;

namespace ServeRick
{
    static class Research
    {

        internal static string Test1()
        {
            return @"
%div#content.right
    .left.column
        %h2 Welcome to our site!
        %p= print_information
    .right.column
        = render :partial => sidebar
";
        }

        internal static string PageSource()
        {
            return @"
%html
    %head
        %title Testing of HAML compilation
    %body
        #test
            = print ""hello world!""
        .long
            Some extra loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong context
";
        }

        static ResponseHandler generateHandler()
        {
            var type = typeof(Response);
            var write = type.GetMethod("Write");

            var param = Expression.Parameter(type);
            var data1 = Expression.Constant("<div class='test'>");
            var data3 = Expression.Constant("</div>");

            var dateType = typeof(DateTime);
            var now = dateType.GetProperty("Now").GetMethod;
            var toString = dateType.GetMethod("ToString", new Type[0]);

            var data2 = Expression.Call(Expression.Call(null, now), toString);

            var write1 = Expression.Call(param, write, data1);
            var write2 = Expression.Call(param, write, data2);
            var write3 = Expression.Call(param, write, data3);

            var viewBlock = Expression.Block(write1, write2, write3);


            return Expression.Lambda<ResponseHandler>(viewBlock, param).Compile();
        }

    }
}
