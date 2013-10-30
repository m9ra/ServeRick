using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServeRick.Compiling
{
    class ContainerInstruction:Instruction
    {
        private Type _type;

        public IEnumerable<Instruction> Pairs { get; private set; }

        public override Type ReturnType
        {
            get { return _type; }
        }

        internal ContainerInstruction(IEnumerable<Instruction> pairs)
        {
            var copy=pairs.ToArray();
            Pairs=copy;
            if (copy.Length == 0)
            {
                _type = typeof(IEnumerable<>);
            }
            else
            {
                _type = typeof(IEnumerable<>).MakeGenericType(copy[0].ReturnType);
            }
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
