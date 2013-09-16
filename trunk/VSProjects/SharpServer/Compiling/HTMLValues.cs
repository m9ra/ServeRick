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
    delegate object PartialEvaluator();

    class HTMLValues : InstructionVisitor
    {
        static readonly Type ThisType = typeof(HTMLValues);
        static readonly MethodInfo StringConcat = typeof(string).GetMethod("Concat", new[] { typeof(object[]) });        
               

        static HTMLValues _creator = new HTMLValues();

        private Expression _result;

        public static Expression GetValue(Instruction instruction)
        {
            if (instruction == null)
                return null;

            instruction.VisitMe(_creator);

            var result = _creator._result;
            _creator._result = null;

            if (instruction.IsStatic())
            {
                result = precomputeExpression(result);
            }

            return result;
        }

        private static Expression precomputeExpression(Expression staticExpression)
        {
            var evaluator = Expression.Lambda<PartialEvaluator>(staticExpression).Compile();
            var precomputed = evaluator();

            return Expression.Constant(precomputed);
        }

        public override void VisitInstruction(Instruction x)
        {
            throw new NotImplementedException();
        }

        public override void VisitCall(CallInstruction x)
        {
            var args = new List<Expression>();

            foreach (var arg in x.Arguments)
            {
                args.Add(GetValue(arg));
            }

            result(callMethod(x.Method.Info, args));
        }

        public override void VisitConstant(ConstantInstruction x)
        {
            result(Expression.Constant(x.Constant));
        }

        public override void VisitPair(PairInstruction x)
        {
            var key = GetValue(x.Key);
            var value = GetValue(x.Value);

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
                pairValues.Add(GetValue(pair));
            }

            result(pairsToContainer(pairValues));
        }

        public override void VisitTag(TagInstruction x)
        {
            var name = GetValue(x.Name);
            var content = GetValue(x.Content);
            var attributes = GetValue(x.Attributes);

            var stringyAttributes = attributes == null ? Expression.Constant("") : attributesToString(attributes);

            if (content == null)
            {
                resultConcat("<", name, stringyAttributes, "/>");
            }
            else
            {
                resultConcat(
                    "<", name, stringyAttributes, ">", content,
                    "</", name, ">"
                    );
            }
        }

        public override void VisitConcat(ConcatInstruction x)
        {
            var chunks = new List<Expression>();

            foreach (var chunk in x.Chunks)
            {
                chunks.Add(GetValue(chunk));
            }

            resultConcat(chunks.ToArray());
        }
        
        private Expression attributesToString(Expression attributesContainer)
        {
            return callMethod("AttributesToString", attributesContainer);
        }

        internal Expression pairsToContainer(IEnumerable<Expression> pairs)
        {
            return callMethod("PairsToContainer", pairs);
        }

        private Expression callMethod(string name, params Expression[] args)
        {
            return callMethod(name, (IEnumerable<Expression>)args);
        }

        private Expression callMethod(string name,IEnumerable<Expression> args){
            var method = ResponseHandlerProvider.WebMethods.GetMethod(name);
            return callMethod(method.Info, args);
        }

        private Expression callMethod(MethodInfo methodInfo, IEnumerable<Expression> args)
        {
            var matcher = new MethodMatcher(methodInfo, args);
            return matcher.CreateCall();
        }

        private void resultConcat(params object[] chunks)
        {
            var chunkExprs = new List<Expression>();
            foreach (var chunk in chunks)
            {
                if (chunk == null)
                    continue;

                var expr = chunk as Expression;

                if (expr == null)
                    expr = Expression.Constant(chunk);

                chunkExprs.Add(expr);
            }

            var array = Expression.NewArrayInit(typeof(object), chunkExprs.ToArray());

            result(Expression.Call(null, StringConcat, array));
        }

        private void result(Expression result)
        {
            _result = result;
        }
    }
}
