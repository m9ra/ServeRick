using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace ServeRick.Compiling.Instructions
{
    class ConstructorInstruction:Instruction
    {
        internal readonly ConstructorInfo Constructor;
        internal readonly Instruction[] Arguments;

        public Type ConstructedType { get { return Constructor.DeclaringType; } }

        public ConstructorInstruction(ConstructorInfo constructor, Instruction[] arguments)
        {
            Constructor = constructor;
            Arguments = arguments;
        }


        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitConstructor(this);
        }

        internal override bool IsStatic()
        {
            return false;
        }

        public override Type ReturnType
        {
            get { return ConstructedType; }
        }
    }
}
