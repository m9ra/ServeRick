using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class ResponseInstruction : Instruction
    {
        public ResponseInstruction()
        {
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitResponse(this);
        }

        internal override bool IsStatic()
        {
            return false;
        }
    }
}
