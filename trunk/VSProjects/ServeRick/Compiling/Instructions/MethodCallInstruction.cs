using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace ServeRick.Compiling.Instructions
{
    class MethodCallInstruction : Instruction
    {
        internal readonly MethodInfo Method;
        internal readonly Instruction[] Arguments;
        internal readonly Instruction ThisObject;

        public override Type ReturnType
        {
            get { return Method.ReturnType; }
        }

        internal MethodCallInstruction(Instruction thisObject, MethodInfo method, params Instruction[] args)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            Method = method;
            Arguments = args;
            ThisObject = thisObject;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitMethodCall(this);
        }

        internal override bool IsStatic()
        {
            return TestStatic(ThisObject) && TestStatic(Arguments);
        }
    }
}
