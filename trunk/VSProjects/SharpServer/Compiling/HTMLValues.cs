using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;
using System.Reflection;

using SharpServer.Compiling.Instructions;

namespace SharpServer.Compiling
{
    class HTMLValues : InstructionVisitor
    {
        static readonly Type ThisType = typeof(HTMLValues);
        static readonly MethodInfo StringConcat = typeof(string).GetMethod("Concat", new[] { typeof(object[]) });

        private readonly HTMLCompiler _compiler;

        private Expression _result;

        internal HTMLValues(HTMLCompiler compiler)
        {
            _compiler = compiler;
        }

        public static Expression GetValue(Instruction instruction, HTMLCompiler compiler)
        {
            if (instruction == null)
                return null;

            var creator = new HTMLValues(compiler);
            instruction.VisitMe(creator);

            var result = creator._result;
            creator._result = null;
            return result;
        }
             
        #region Visitor overrides

        public override void VisitInstruction(Instruction x)
        {
            throw new NotImplementedException();
        }

        public override void VisitTag(TagInstruction x)
        {
            throw new NotSupportedException("Tag instruction is not RValue");
        }

        public override void VisitConcat(ConcatInstruction x)
        {
            throw new NotSupportedException("Concat instruction is not RValue");
        }

        public override void VisitCall(CallInstruction x)
        {
            var args = new List<Expression>();

            foreach (var arg in x.Arguments)
            {
                args.Add(getValue(arg));
            }

            result(_compiler.CallMethod(x.IsStatic(),x.Method.Info, args));
        }

        public override void VisitConstant(ConstantInstruction x)
        {
            result(Expression.Constant(x.Constant));
        }

        public override void VisitResponse(ResponseInstruction x)
        {
            result(_compiler.ResponseParameter);
        }

        public override void VisitParam(ParamInstruction x)
        {
            result(_compiler.Param(x.Declaration.Name));
        }

        public override void VisitPair(PairInstruction x)
        {
            var key = getValue(x.Key);
            var value = getValue(x.Value);

            var argTypes = new[] { key.Type, value.Type };

            var keyPairType = typeof(Tuple<,>).MakeGenericType(argTypes);
            var keyPairCtor = keyPairType.GetConstructor(argTypes);

            result(Expression.New(keyPairCtor, key, value));
        }

        public override void VisitContainer(ContainerInstruction x)
        {
            var pairValues = new List<Expression>();

            foreach (var pair in x.Pairs)
            {
                pairValues.Add(getValue(pair));
            }

            result(pairsToContainer(pairValues,x.IsStatic()));
        }

        #endregion


        #region Private utilities

        internal Expression pairsToContainer(IEnumerable<Expression> pairs,bool precompute)
        {
            return _compiler.CallMethod(precompute,"PairsToContainer", pairs);
        }

        private void result(Expression result)
        {
            _result = result;
        }

        private Expression getValue(Instruction instruction)
        {
            return _compiler.GetValue(instruction);
        }

        #endregion
    }
}
