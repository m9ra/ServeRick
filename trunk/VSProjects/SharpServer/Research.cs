using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Diagnostics;

namespace SharpServer
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

        internal static string TestRender()
        {
            return @"
#test
    = print ""hello world!""
";
        }


        static void directWrite(Response response)
        {
            response.Write("<div class='test'>");
            response.Write(DateTime.Now.ToString());
            response.Write("</div>");
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

        static void benchMark()
        {
            var view = generateHandler();

            var LOOP_COUNT = 100000;

            var response = new Response();
            //omitting initialization
            directWrite(response);

            var w = Stopwatch.StartNew();
            for (int i = 0; i < LOOP_COUNT; ++i)
                directWrite(response);
            w.Stop();

            Console.WriteLine("Direct write: {0}ms", w.ElapsedMilliseconds);

            response = new Response();
            //omitting initialization
            view(response);

            w = Stopwatch.StartNew();
            for (int i = 0; i < LOOP_COUNT; ++i)
                view(response);
            w.Stop();

            Console.WriteLine("Generated write: {0}ms", w.ElapsedMilliseconds);
        }
    }
}
