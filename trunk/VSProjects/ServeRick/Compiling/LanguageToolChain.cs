using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Compiling
{

    public delegate void EmitDelegate(string source,Emitter emitter);

    class LanguageToolChain
    {
        public readonly string Language;     
        public readonly EmitDelegate EmitDelegate;

        public LanguageToolChain(string language, EmitDelegate compile)
        {
            Language = language;
            EmitDelegate = compile;
        }
    }
}
