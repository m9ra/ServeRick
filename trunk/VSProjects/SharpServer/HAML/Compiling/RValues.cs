using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using SharpServer.Compiling;

namespace SharpServer.HAML.Compiling
{
    abstract class RValue
    {
        protected readonly Emitter E;
        internal abstract Expression ToExpression();

        internal RValue(Emitter emitter)
        {
            E = emitter;
        }
    }


    class CallValue : RValue
    {
        internal readonly string CallName;
        internal readonly RValue[] Args;

        internal CallValue(string callName, RValue[] args, Emitter emitter) :
            base(emitter)
        {
            CallName = callName;
            Args = args;
        }

        internal override Expression ToExpression()
        {
            var argExprs = new List<Expression>();
            foreach (var arg in Args)
            {
                argExprs.Add(arg.ToExpression());
            }

            return E.Call(CallName, argExprs.ToArray());            
        }
    }

    class LiteralValue : RValue
    {
        internal readonly string Literal;
        internal LiteralValue(string literal, Emitter emitter) :
            base(emitter)
        {
            Literal = literal;
        }


        internal override Expression ToExpression()
        {
            return E.Constant(Literal);
        }
    }
}
