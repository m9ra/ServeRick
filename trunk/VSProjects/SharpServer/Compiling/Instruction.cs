using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling
{
    public abstract class Instruction
    {
        internal abstract void VisitMe(InstructionVisitor visitor);

        /// <summary>
        /// Determine that instruction and its children can be evaluated staticly (without any resources available)
        /// </summary>
        /// <returns>True if instruction can be evaluated staticly, false otherwise</returns>
        internal abstract bool IsStatic();


        protected bool TestStatic(IEnumerable<Instruction> children)
        {
            foreach (var child in children)
            {
                if (child!=null && !child.IsStatic())
                {
                    return false;
                }
            }
            return true;
        }

        protected bool TestStatic(params Instruction[] children)
        {
            return TestStatic((IEnumerable<Instruction>)children);
        }
    }
}
