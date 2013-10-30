using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

namespace ServeRick.Compiling
{
    public class ResponseHandlerProvider
    {
        private static readonly Dictionary<string, LanguageToolChain> _toolChains = new Dictionary<string, LanguageToolChain>();

        internal static readonly WebMethods CompilerHelpers = new WebMethods(typeof(CompilerHelpers));

        private readonly WebMethods WebHelpers;


        internal ResponseHandlerProvider(WebMethods webHelpers)
        {
            WebHelpers = webHelpers;
        }

        internal static void Register(LanguageToolChain toolChain)
        {
            _toolChains[toolChain.Language] = toolChain;
        }

        public ResponseHandler Compile(string language, string source)
        {
            var toolChain = _toolChains[language];

            var emitter = new Emitter(WebHelpers);

            toolChain.EmitDelegate(source, emitter);
            if (emitter.HasError)
            {
                //TODO error handling

                var errorBytes = Encoding.UTF8.GetBytes(emitter.ErrorMessage);

                return (response) => response.Write(errorBytes);
            }

            var instructions = emitter.GetEmittedResult();
            return HTMLCompiler.Compile(instructions,emitter.Parameters);
        }
    }
}
