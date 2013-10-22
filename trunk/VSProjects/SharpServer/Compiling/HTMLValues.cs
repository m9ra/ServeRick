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

        private ExpressionUnit _result;

        internal HTMLValues(HTMLCompiler compiler)
        {
            _compiler = compiler;
        }

        public static ExpressionUnit GetValue(Instruction instruction, HTMLCompiler compiler)
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
            var args = new List<ExpressionUnit>();
            foreach (var arg in x.Arguments)
            {
                args.Add(getValue(arg));
            }

            result(_compiler.CallMethod(x.IsStatic(), x.Method.Info, args));
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
            var deps = emptyDepsContainer();
            var key = getValue(x.Key, deps);
            var value = getValue(x.Value, deps);

            var argTypes = new[] { key.Type, value.Type };

            var keyPairType = typeof(Tuple<,>).MakeGenericType(argTypes);
            var keyPairCtor = keyPairType.GetConstructor(argTypes);

            result(Expression.New(keyPairCtor, key, value), deps);
        }

        private List<Expression> emptyDepsContainer()
        {
            return new List<Expression>();
        }

        public override void VisitContainer(ContainerInstruction x)
        {
            var pairValues = new List<ExpressionUnit>();

            foreach (var pair in x.Pairs)
            {
                pairValues.Add(getValue(pair));
            }

            result(pairsToContainer(pairValues, x.IsStatic()));
        }

        public override void VisitIf(IfInstruction x)
        {
            //TODO resolve void branches
            var temporary = _compiler.CreateTemporary(x.ReturnType);

            var deps = emptyDepsContainer();

            var condition = getValue(x.Condition, deps);
            var ifBranch = compileBranch(x.IfBranch, temporary, deps);
            var elseBranch = compileBranch(x.ElseBranch, temporary, deps);

            Expression ifStatement;
            if (elseBranch == null)
            {
                ifStatement = Expression.IfThen(condition, ifBranch);
            }
            else
            {
                ifStatement = Expression.IfThenElse(condition, ifBranch, elseBranch);
            }

            deps.Add(ifStatement);

            result(temporary, deps);
        }

        #endregion


        #region Private utilities
        private Expression compileBranch(Instruction branch, ParameterExpression tempVariable, List<Expression> dependencies)
        {
            if (branch == null)
                return null;

            if (branch.ReturnType == typeof(void))
            {
                throw new NotImplementedException();
            }
            else
            {
                var valueUnit = getValue(branch, dependencies);
                return Expression.Assign(tempVariable, valueUnit);
            }
        }

        internal ExpressionUnit pairsToContainer(IEnumerable<ExpressionUnit> pairs, bool precompute)
        {
            return _compiler.CallMethod(precompute, "PairsToContainer", pairs);
        }

        private void result(Expression resultExpression, IEnumerable<Expression> dependencies)
        {
            result(resultExpression, dependencies.ToArray());
        }

        private void result(Expression resultExpression, params Expression[] dependencies)
        {
            result(new ExpressionUnit(resultExpression, dependencies));
        }

        private void result(ExpressionUnit resultUnit)
        {
            _result = resultUnit;
        }

        private ExpressionUnit getValue(Instruction instruction)
        {
            return _compiler.GetValue(instruction);
        }

        /// <summary>
        /// Get value of instruction and fill given list with dependencies needed for returned value
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="valueDependencies"></param>
        /// <returns></returns>
        private Expression getValue(Instruction instruction, List<Expression> valueDependencies)
        {
            var unit = getValue(instruction);
            valueDependencies.AddRange(unit.Dependencies);
            return unit.Value;
        }

        #endregion
    }
}
