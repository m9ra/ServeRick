using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Reflection.Emit;

using System.Diagnostics;

using System.Net;

using ServeRick.Networking;
using ServeRick.Memory;

using ServeRick.Compiling;
using ServeRick.Languages.HAML;

namespace ServeRick
{
    public static class ServerEnvironment
    {
        static List<WebApplication> _applications = new List<WebApplication>();

        public static void Start()
        {
            LoadToolchains();

            if (_applications.Count != 1)
                throw new NotImplementedException();

            var netConfig = new NetworkConfiguration(4000, IPAddress.Any);
            var memConfig = new MemoryConfiguration(4096, 2 << 20);

            var server = new HttpServer(_applications[0], netConfig, memConfig);
            server.Start();
        }

        public static void LoadToolchains()
        {
            var hamlChain = new LanguageToolChain("haml", Languages.HAML.Compiler.Compile);
            ResponseHandlerProvider.Register(hamlChain);

            var scssChain = new LanguageToolChain("scss", Languages.SCSS.Compiler.Compile);
            ResponseHandlerProvider.Register(scssChain);
        }

        public static void AddApplication(WebApplication webApp)
        {
            _applications.Add(webApp);
        }
    }
}
