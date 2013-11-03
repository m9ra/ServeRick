using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace ServeRick.Compiling.Instructions
{
    class FieldInstruction : Instruction
    {
        internal readonly FieldInfo Field;
        internal readonly Instruction ThisObject;

        public override Type ReturnType
        {
            get { return Field.FieldType; }
        }

        internal FieldInstruction(Instruction thisObject, FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("method");

            Field = field;
            ThisObject = thisObject;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitField(this);
        }

        internal override bool IsStatic()
        {
            return ThisObject.IsStatic();
        }
    }
}
