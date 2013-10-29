using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class WhileInstruction:Instruction
    {
        internal readonly Instruction Condition;

        internal readonly Instruction LoopBlock;

        internal WhileInstruction(Instruction condition, Instruction loopBlock)
        {
            Condition = condition;
            LoopBlock = loopBlock;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitWhile(this);
        }

        internal override bool IsStatic()
        {
            return TestStatic(Condition, LoopBlock);
        }

        public override Type ReturnType
        {
            get { return LoopBlock.ReturnType; }
        }
    }
}
