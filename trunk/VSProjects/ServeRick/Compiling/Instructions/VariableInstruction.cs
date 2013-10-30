using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Compiling
{
    /// <summary>
    /// Represents variable occurance. Occurance is resolved according to instance
    /// (NameHint is not used for variable resolving)
    /// </summary>
    class VariableInstruction:Instruction
    {
        internal readonly Type VariableType;

        /// <summary>
        /// Name hint for variable is used for debugging purposes only
        /// </summary>
        internal readonly string NameHint;

        internal VariableInstruction(string nameHint, Type variableType)
        {
            NameHint = nameHint;
            VariableType = variableType;
        }

        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitVariable(this);
        }

        internal override bool IsStatic()
        {
            //TODO resolve semantic
            return false;
        }

        public override Type ReturnType
        {
            get { return VariableType; }
        }
    }
}
