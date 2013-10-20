using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class ParamInstruction : Instruction
    {
        public readonly ParamDeclaration Declaration;

        public ParamInstruction(ParamDeclaration declaration)
        {
            Declaration=declaration;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitParam(this);
        }

        internal override bool IsStatic()
        {
            return false;
        }
    }
}
