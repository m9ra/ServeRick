using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Reflection.Emit;

using System.Diagnostics;

using Irony.Parsing;

using SharpServer.Compiling;

namespace SharpServer
{

    class Program
    {

        static void initialize()
        {
            var hamlGrammar = new HAML.Grammar();
            var lang = new LanguageData(hamlGrammar);
            var parser = new Parser(lang);

            var hamlChain = new LanguageToolChain("haml", parser, HAML.Compiler.Compile);

            ResponseHandlerProvider.Register(hamlChain);
        }


        static void Main(string[] args)
        {
            initialize();

            ResponseHandler handler = null;
            var response = new Response();

            var w = Stopwatch.StartNew();
            handler = ResponseHandlerProvider.GetHandler(
                    "haml",
                    Research.TestRender()
                    );

            if (handler != null)
            {
                handler(response);
            }

            w.Stop();

            Console.WriteLine("Time elapsed: {0}ms", w.ElapsedMilliseconds);
            Console.WriteLine(response.GetResult());
            Console.ReadKey();
        }




    }
}
