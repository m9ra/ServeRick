using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class IfInstruction : Instruction
    {
        public readonly Instruction Condition;

        public readonly Instruction IfBranch;

        public readonly Instruction ElseBranch;

        private readonly Type _type;

        public override Type ReturnType { get { return _type; } }

        internal IfInstruction(Instruction condition, Instruction ifBranch, Instruction elseBranch)
        {
            Condition = condition;
            IfBranch = ifBranch;
            ElseBranch = elseBranch;

            _type = TypeHelper.FindBaseClassWith(IfBranch, ElseBranch);
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitIf(this);
        }

        internal override bool IsStatic()
        {
            return TestStatic(Condition, IfBranch, ElseBranch);
        }
    }
}
