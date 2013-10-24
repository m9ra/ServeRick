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

        private readonly List<Expression> _emitted = new List<Expression>();

        private readonly HTMLCompiler _compiler;

        private HTMLValues(HTMLCompiler compiler)
        {
            _compiler = compiler;
        }

        public static Expression GetValue(Instruction instruction, HTMLCompiler compiler)
        {
            if (instruction == null)
                return null;

            var creator = new HTMLValues(compiler);
            instruction.VisitMe(creator);
            var expression = compiler.CompileBlock(creator._emitted);

            return expression;
        }

        #region Visitor overrides

        public override void VisitInstruction(Instruction x)
        {
            throw new NotImplementedException();
        }

        public override void VisitTag(TagInstruction x)
        {
            var name = getValue(x.Name);
            var attributes = getValue(x.Attributes);

            var stringyAttributes = attributes == null ? Expression.Constant("") : attributesToString(attributes, x.Attributes.IsStatic());

            if (x.Content == null)
            {
                emitWriteConcat(x, "<", name, stringyAttributes, "/>");
            }
            else
            {
                emitWriteConcat(x, "<", name, stringyAttributes, ">");
                x.Content.VisitMe(this);
                emitWriteConcat(x, "</", name, ">");
            }
        }

        public override void VisitSequence(SequenceInstruction x)
        {
            foreach (var chunk in x.Chunks)
            {
                var value = getValue(chunk);
                emit(value);
            }
        }

        public override void VisitCall(CallInstruction x)
        {
            var args = new List<Expression>();

            foreach (var arg in x.Arguments)
            {
                args.Add(getValue(arg));
            }

            emit(_compiler.CallMethod(x.IsStatic(), x.Method.Info, args));
        }

        public override void VisitConstant(ConstantInstruction x)
        {
            emit(Expression.Constant(x.Constant));
        }

        public override void VisitResponse(ResponseInstruction x)
        {
            emit(_compiler.ResponseParameter);
        }

        public override void VisitParam(ParamInstruction x)
        {
            emit(_compiler.Param(x.Declaration.Name));
        }

        public override void VisitPair(PairInstruction x)
        {
            var key = getValue(x.Key);
            var value = getValue(x.Value);

            var argTypes = new[] { key.Type, value.Type };

            var keyPairType = typeof(Tuple<,>).MakeGenericType(argTypes);
            var keyPairCtor = keyPairType.GetConstructor(argTypes);

            emit(Expression.New(keyPairCtor, key, value));
        }

        public override void VisitContainer(ContainerInstruction x)
        {
            var pairValues = new List<Expression>();

            foreach (var pair in x.Pairs)
            {
                pairValues.Add(getValue(pair));
            }

            emit(pairsToContainer(pairValues, x.IsStatic()));
        }

        public override void VisitIf(IfInstruction x)
        {
            var condition = getValue(x.Condition);

            //TODO needs to be compiled as satement
            var tmpVariable = _compiler.GetTemporaryVariable(x.ReturnType);
            var ifBranch = compileBranch(x.IfBranch, tmpVariable);
            var elseBranch = compileBranch(x.ElseBranch, tmpVariable);

            Expression ifStatement;
            if (elseBranch == null)
            {
                ifStatement = Expression.IfThen(condition, ifBranch);
            }
            else
            {
                ifStatement = Expression.IfThenElse(condition, ifBranch, elseBranch);
            }

            emit(Expression.Block(ifStatement, tmpVariable));
        }

        public override void VisitWrite(WriteInstruction x)
        {
            var value = getValue(x.Data);
            emitWrite(value);
        }

        #endregion

        #region Private utilities

        private Expression compileBranch(Instruction branchInstruction, ParameterExpression outputVar)
        {
            if (branchInstruction == null)
                return null;

            var branch = getValue(branchInstruction);
            if (branch.Type == typeof(void))
            {
                return branch;
            }
            else
            {
                return Expression.Assign(outputVar, branch);
            }
        }

        internal Expression pairsToContainer(IEnumerable<Expression> pairs, bool precompute)
        {
            return _compiler.CallMethod(precompute, "PairsToContainer", pairs);
        }

        private void emitWriteConcat(Instruction emitingInstruction, params object[] chunks)
        {
            var parts = new List<Expression>();
            foreach (var chunk in chunks)
            {
                if (chunk == null)
                    continue;

                var part = chunk as Expression;

                if (part == null)
                    part = Expression.Constant(chunk);

                parts.Add(part);
            }

            var array = Expression.NewArrayInit(typeof(string), parts);
            var concatenation = _compiler.CallMethod(emitingInstruction.IsStatic(), "Concat", array);
            emitWrite(concatenation);
        }

        private Expression attributesToString(Expression attributesContainer, bool precompute)
        {
            return _compiler.CallMethod(precompute, "AttributesToString", attributesContainer);
        }

        private Expression getValue(Instruction instruction)
        {
            return _compiler.GetValue(instruction);
        }

        private void emitWrite(Expression statement)
        {
            var writeBytes = _compiler.WriteBytes(statement);
            emit(writeBytes);
        }

        private void emit(Expression statement)
        {
            _emitted.Add(statement);
        }
        #endregion
    }
}