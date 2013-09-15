using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class PairInstruction:Instruction
    {
        internal readonly Instruction Key;
        internal readonly Instruction Value;

        internal PairInstruction(Instruction key, Instruction value) {
            Key = key;
            Value = value;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitPair(this);
        }

        internal override bool IsStatic()
        {
            return TestStatic(Key, Value);
        }
    }
}
