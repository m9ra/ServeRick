using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Reflection.Emit;

using System.Diagnostics;

using System.Net;

using Irony.Ast;
using Irony.Parsing;

using SharpServer.Networking;
using SharpServer.Memory;

using SharpServer.Compiling;
using SharpServer.HAML;

namespace SharpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            LoadToolchains();

            var netConfig = new NetworkConfiguration(4000, IPAddress.Any);
            var memConfig = new MemoryConfiguration(4096, 2 << 20);
            var controllers = NarioShop.GetManager();

            var server = new HttpServer(controllers, netConfig, memConfig);
            server.Start();

            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();

                switch (keyInfo.KeyChar)
                {
                    case 't':
                        Log.TraceDisabled = !Log.TraceDisabled;
                        break;
                    case 'n':
                        Log.NoticeDisabled = !Log.NoticeDisabled;
                        break;
                }

            } while (keyInfo.Key != ConsoleKey.Escape);
            Environment.Exit(0);   
        }

        static void LoadToolchains()
        {
            var hamlGrammar = new HAML.Grammar();
            var lang = new LanguageData(hamlGrammar);
            var parser = new Parser(lang);

            var hamlChain = new LanguageToolChain("haml", parser, HAML.Compiler.Compile);

            ResponseHandlerProvider.Register(hamlChain);
        }
    }
}
