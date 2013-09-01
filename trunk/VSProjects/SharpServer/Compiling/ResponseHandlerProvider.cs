using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

namespace SharpServer.Compiling
{
    static class ResponseHandlerProvider
    {
        static readonly Dictionary<string, LanguageToolChain> _toolChains = new Dictionary<string, LanguageToolChain>();
        
        public static void Register(LanguageToolChain toolChain){
            _toolChains[toolChain.Language]=toolChain;
        }

        public static ResponseHandler GetHandler(string language,string source)
        {
            var toolChain= _toolChains[language];

            var tree=toolChain.Parser.Parse(source);
            

            Output.DisplayTree(tree);

            if (tree.Root != null)
            {
                var emitter = new Emitter();
                toolChain.Compile(tree.Root, emitter);

                return emitter.GetResult();
            }
            else
            {
                return null;
            }
        }

    }
}
