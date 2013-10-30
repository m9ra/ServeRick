using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Compiling.Instructions
{
    class ParamInstruction : Instruction
    {
        public readonly ParamDeclaration Declaration;

        public override Type ReturnType{get { return Declaration.Type; }}

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
