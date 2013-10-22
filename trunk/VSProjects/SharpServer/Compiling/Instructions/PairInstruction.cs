using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class PairInstruction:Instruction
    {
        private readonly Type _type;

        internal readonly Instruction Key;
        internal readonly Instruction Value;

        public override Type ReturnType
        {
            get { return _type; }
        }

        internal PairInstruction(Instruction key, Instruction value) {
            Key = key;
            Value = value;

            _type = typeof(KeyValuePair<,>).MakeGenericType(key.ReturnType, value.ReturnType);
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
