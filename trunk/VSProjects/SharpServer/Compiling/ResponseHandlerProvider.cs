using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

namespace SharpServer.Compiling
{
    public static class ResponseHandlerProvider
    {
        static readonly Dictionary<string, LanguageToolChain> _toolChains = new Dictionary<string, LanguageToolChain>();

        internal static readonly WebMethods WebMethods=new WebMethods();

        internal static void Register(LanguageToolChain toolChain){
            _toolChains[toolChain.Language]=toolChain;
        }

        public static ResponseHandler GetHandler(string language,string source)
        {
            var toolChain= _toolChains[language];

            var tree=toolChain.Parser.Parse(source);
            Output.DisplayTree(tree);

            if (tree.Root != null)
            {
                var emitter = new Emitter(WebMethods);
                toolChain.Compile(tree.Root, emitter);

                var instructions = emitter.GetEmittedResult();
                return HTMLCompiler.Compile(instructions);
            }
            else
            {
                return null;
            }
        }
    }
}
