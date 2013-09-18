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

        internal static readonly WebMethods CompilerHelpers=new WebMethods(typeof(CompilerHelpers));

        internal static void Register(LanguageToolChain toolChain){
            _toolChains[toolChain.Language]=toolChain;
        }

        public static ResponseHandler GetHandler(string language,string source,WebMethods helperMethods)
        {
            var toolChain= _toolChains[language];

            var tree=toolChain.Parser.Parse(source);
            Output.DisplayTree(tree);

            if (tree.Root != null)
            {
                var emitter = new Emitter(helperMethods);
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
