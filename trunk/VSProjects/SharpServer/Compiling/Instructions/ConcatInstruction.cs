using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class ConcatInstruction:Instruction
    {
        public IEnumerable<Instruction> Chunks { get; private set; }

        public ConcatInstruction(IEnumerable<Instruction> statements)
        {
            Chunks = statements.ToArray();
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitConcat(this);
        }

        internal override bool IsStatic()
        {
            return TestStatic(Chunks);
        }
    }
}
