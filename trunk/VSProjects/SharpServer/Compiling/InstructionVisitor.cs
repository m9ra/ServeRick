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
        public abstract void VisitInstruction(Instruction x);

        public virtual void VisitConstant(ConstantInstruction x)
        {
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

        public virtual void VisitMethodCall(MethodCallInstruction x)
        {
            foreach (var arg in x.Arguments)
            {
                arg.VisitMe(this);
            }
            VisitInstruction(x.ThisObject);
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

        public virtual void VisitResponse(ResponseInstruction x)
        {
            VisitInstruction(x);
        }

        public virtual void VisitParam(ParamInstruction x)
        {
            VisitInstruction(x);
        }

        public virtual void VisitWrite(WriteInstruction x)
        {
            x.Data.VisitMe(this);
            VisitInstruction(x);
        }

        public virtual void VisitIf(IfInstruction x)
        {
            x.Condition.VisitMe(this);

            x.IfBranch.VisitMe(this);

            if (x.ElseBranch != null)
                x.ElseBranch.VisitMe(this);

            VisitInstruction(x);
        }
    }
}
