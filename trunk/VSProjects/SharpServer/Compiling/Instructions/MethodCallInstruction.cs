using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace SharpServer.Compiling.Instructions
{
    class MethodCallInstruction : Instruction
    {
        internal readonly WebMethod Method;
        internal readonly Instruction[] Arguments;
        internal readonly Instruction ThisObject;

        public override Type ReturnType
        {
            get { return Method.Info.ReturnType; }
        }

        internal MethodCallInstruction(Instruction thisObject, WebMethod method, params Instruction[] args)
        {            
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
