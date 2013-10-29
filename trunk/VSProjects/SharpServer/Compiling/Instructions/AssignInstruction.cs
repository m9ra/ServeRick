using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class AssignInstruction:Instruction
    {
        internal readonly VariableInstruction Target;

        internal readonly Instruction AssignedValue;

        internal AssignInstruction(VariableInstruction target, Instruction assignedValue)
        {
            Target = target;
            AssignedValue = assignedValue;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitAssign(this);
        }

        internal override bool IsStatic()
        {
            //TODO repair static semantic for external state
            return false;
        }

        public override Type ReturnType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
