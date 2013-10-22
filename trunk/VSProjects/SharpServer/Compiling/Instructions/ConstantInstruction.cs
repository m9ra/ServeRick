using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class ConstantInstruction:Instruction
    {
        public readonly object Constant;

        private readonly Type _type;

        public override Type ReturnType { get { return _type; } }

        public ConstantInstruction(object constant, Type explicitType)
        {
            Constant = constant;
            if (Constant == null)
            {
                _type = explicitType;
            }
            else
            {
                _type = constant.GetType();
            }
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitConstant(this);
        }

        internal override bool IsStatic()
        {
            return true;
        }
    }
}
