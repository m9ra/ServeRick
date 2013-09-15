using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpServer.Compiling
{
    class ContainerInstruction:Instruction
    {
        public IEnumerable<Instruction> Pairs { get; private set; }
        internal ContainerInstruction(IEnumerable<Instruction> pairs)
        {
            Pairs=pairs.ToArray();
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitContainer(this);
        }

        internal override bool IsStatic()
        {
            return TestStatic(Pairs);
        }
    }
}
