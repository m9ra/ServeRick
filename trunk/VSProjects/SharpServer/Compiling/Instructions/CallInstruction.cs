using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace SharpServer.Compiling.Instructions
{
    class CallInstruction:Instruction
    {
        internal readonly WebMethod Method;
        internal readonly Instruction[] Arguments;

        public override Type ReturnType
        {
            get { return Method.Info.ReturnType; }
        }

        internal CallInstruction(WebMethod method,params Instruction[] args)
        {
            Method = method;
            Arguments = args;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitCall(this);
        }

        internal override bool IsStatic()
        {
            return TestStatic(Arguments);
        }
    }
}
