using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Compiling.Instructions
{
    class SequenceInstruction:Instruction
    {
        public IEnumerable<Instruction> Chunks { get; private set; }

        public override Type ReturnType
        {
            get { return typeof(string); }
        }

        public SequenceInstruction(IEnumerable<Instruction> statements)
        {
            Chunks = statements.ToArray();
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitSequence(this);
        }

        internal override bool IsStatic()
        {
            return TestStatic(Chunks);
        }
    }
}
