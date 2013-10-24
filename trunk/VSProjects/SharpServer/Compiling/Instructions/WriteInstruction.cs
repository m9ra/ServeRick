using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class WriteInstruction : Instruction
    {
        internal readonly Instruction Data;

        public override Type ReturnType
        {
            get { return typeof(void); }
        }

        internal WriteInstruction(Instruction data)
        {
            Data = data;

        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitWrite(this);
        }

        internal override bool IsStatic()
        {
            return false;
        }
    }
}
