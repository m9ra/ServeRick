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
   public static class ServerEnvironment
    {
       static List<WebApplication> _list = new List<WebApplication>();

        public static void Start()
        {
            LoadToolchains();

            if (_list.Count != 1)
                throw new NotImplementedException();

            var netConfig = new NetworkConfiguration(4000, IPAddress.Any);
            var memConfig = new MemoryConfiguration(4096, 2 << 20);
            var controllers = _list[0].CreateManager();

            var server = new HttpServer(controllers, netConfig, memConfig);
            server.Start();
        }

        static void LoadToolchains()
        {
            var hamlGrammar = new HAML.Grammar();
            var lang = new LanguageData(hamlGrammar);
            var parser = new Parser(lang);

            var hamlChain = new LanguageToolChain("haml", parser, HAML.Compiler.Compile);

            ResponseHandlerProvider.Register(hamlChain);
        }

        public static void AddManager(WebApplication webApp)
        {
            _list.Add(webApp);
        }
    }
}
