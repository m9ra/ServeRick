using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

namespace SharpServer.Compiling
{

    public delegate void CompileDelegate(ParseTreeNode source, Emitter emitter);

    class LanguageToolChain
    {
        public readonly string Language;
        public readonly Parser Parser;
        public readonly CompileDelegate Compile;

        public LanguageToolChain(string language, Parser parser, CompileDelegate compile)
        {
            Language = language;
            Parser = parser;
            Compile = compile;
        }
    }
}
