using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpServer.Compiling.Instructions;

namespace SharpServer.Compiling
{
    abstract class InstructionVisitor
    {
        public virtual void VisitInstruction(Instruction x){
        }

        public virtual void VisitConstant(ConstantInstruction x){
            VisitInstruction(x);
        }

        public virtual void VisitCall(CallInstruction x)
        {
            foreach (var arg in x.Arguments)
            {
                arg.VisitMe(this);
            }
            VisitInstruction(x);
        }

        public virtual void VisitConcat(ConcatInstruction x)
        {
            foreach (var statement in x.Chunks)
            {
                statement.VisitMe(this);
            }
            VisitInstruction(x);
        }

        public virtual void VisitTag(TagInstruction x)
        {
            x.Name.VisitMe(this);
            x.Attributes.VisitMe(this);
            if (x.Content != null)
            {
                x.Content.VisitMe(this);
            }

            VisitInstruction(x);
        }

        public virtual void VisitPair(PairInstruction x)
        {
            x.Key.VisitMe(this);
            x.Value.VisitMe(this);

            VisitInstruction(x);
        }

        public virtual void VisitContainer(ContainerInstruction x)
        {
            foreach (var pair in x.Pairs)
            {
                pair.VisitMe(this);
            }

            VisitInstruction(x);
        }
    }
}
