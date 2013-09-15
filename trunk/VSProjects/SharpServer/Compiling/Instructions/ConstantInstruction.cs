using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class ConstantInstruction:Instruction
    {
        public readonly object Constant;

        public ConstantInstruction(object constant)
        {
            Constant = constant;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitConstant(this);
        }

        internal override bool IsStatic()
        {
            return true;
        }
    }
}
