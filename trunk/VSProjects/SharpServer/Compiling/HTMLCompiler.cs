using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using SharpServer.Compiling.Instructions;

namespace SharpServer.Compiling
{
    delegate object PartialEvaluator();

    /// <summary>
    /// Compile instructions into ResponseHandler writing Raw http (without headers)
    /// </summary>
    class HTMLCompiler : InstructionVisitor
    {
        private static readonly Type ResponseType = typeof(Response);
        private static readonly MethodInfo WriteMethod = ResponseType.GetMethod("Write");

        private static readonly MethodInfo ConvertBytesMethod = typeof(HTMLCompiler).GetMethod("ConvertBytes", BindingFlags.Static | BindingFlags.NonPublic);

        internal readonly ParameterExpression ResponseParameter = Expression.Parameter(ResponseType, "response");
        private readonly List<ExpressionUnit> _emitted = new List<ExpressionUnit>();
        private readonly StringBuilder _staticWrites = new StringBuilder();

        private List<ParameterExpression> _temporaryVariables = new List<ParameterExpression>();
        private readonly Dictionary<string, ParameterExpression> _paramStorages = new Dictionary<string, ParameterExpression>();

        /// <summary>
        /// Run compilation of given instruction (All write instructions are sended to output in pure HTML)
        /// </summary>
        /// <param name="root">Instruction to be compiled</param>
        /// <returns>Compiled response handler</returns>
        public static ResponseHandler Compile(Instruction root, IEnumerable<ParamDeclaration> parameters)
        {
            var compiler = new HTMLCompiler(parameters);
            root.VisitMe(compiler);

            return compiler.compile();
        }

        private HTMLCompiler(IEnumerable<ParamDeclaration> parameters)
        {
            foreach (var declaration in parameters)
            {
                var name = declaration.Name;
                var storage = Expression.Variable(declaration.Type, name);
                _paramStorages.Add(name, storage);
            }
        }

        public ExpressionUnit GetValue(Instruction instruction)
        {
            if (instruction == null)
                return null;

            var value = HTMLValues.GetValue(instruction, this);

            if (instruction.IsStatic())
            {
                return precomputeExpression(value);
            }

            return value;
        }

        internal Expression Param(string paramName)
        {
            return _paramStorages[paramName];
        }

        public ExpressionUnit CallMethod(bool precompute, string name, params ExpressionUnit[] args)
        {
            return CallMethod(precompute, name, (IEnumerable<ExpressionUnit>)args);
        }

        public ExpressionUnit CallMethod(bool precompute, string name, IEnumerable<ExpressionUnit> args)
        {
            var method = ResponseHandlerProvider.CompilerHelpers.GetMethod(name);
            return CallMethod(precompute, method.Info, args);
        }

        public ExpressionUnit CallMethod(bool precompute, MethodInfo methodInfo, IEnumerable<ExpressionUnit> args)
        {
            var matcher = new MethodMatcher(methodInfo, args);
            var call = matcher.CreateCall();

            if (precompute)
                return precomputeExpression(call);

            return call;
        }

        #region Instruction visitor overrides

        public override void VisitInstruction(Instruction x)
        {
            throw new NotSupportedException("Unsupported instruction visited");
        }

        public override void VisitConstant(ConstantInstruction x)
        {
            var value = GetValue(x);

            emitWrite(value);
        }

        public override void VisitWrite(WriteInstruction x)
        {
            var value = GetValue(x.Data);

            emitWrite(value);
        }

        public override void VisitCall(CallInstruction x)
        {
            var expr = GetValue(x);

            emit(expr);
        }

        public override void VisitMethodCall(MethodCallInstruction x)
        {
            throw new NotImplementedException();
        }

        public override void VisitConcat(ConcatInstruction x)
        {
            foreach (var chunk in x.Chunks)
            {
                chunk.VisitMe(this);
            }
        }


        public override void VisitTag(TagInstruction x)
        {
            var name = GetValue(x.Name);
            var attributes = GetValue(x.Attributes);

            var stringyAttributes = attributes == null ?
                new ExpressionUnit(Expression.Constant("")) :
                attributesToString(attributes, x.Attributes.IsStatic());

            if (x.Content == null)
            {
                resultConcat(x, "<", name, stringyAttributes, "/>");
            }
            else
            {
                resultConcat(x, "<", name, stringyAttributes, ">");
                x.Content.VisitMe(this);
                resultConcat(x, "</", name, ">");
            }
        }

        public override void VisitIf(IfInstruction x)
        {
            var value = GetValue(x);
            emit(value);
        }

        #endregion

        #region Services for compiler

        internal static byte[] ConvertBytes(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        private ResponseHandler compile()
        {
            var compiled = new List<Expression>();
            compileDeclarations(compiled);
            compileChunks(compiled);
            if (compiled.Count == 0)
            {
                return (r) => { };
            }


            var output = Expression.Block(_paramStorages.Values.ToArray(), compiled);
            return Expression.Lambda<ResponseHandler>(output, ResponseParameter).Compile();
        }

        private void compileDeclarations(List<Expression> compiled)
        {
            foreach (var storage in _paramStorages.Values)
            {
                var nameExpr = unit(Expression.Constant(storage.Name));
                var getParam = CallMethod(false, "Param", unit(ResponseParameter), nameExpr);

                getParam = new ExpressionUnit(
                    Expression.Convert(getParam.Value, storage.Type),
                    getParam.Dependencies);

                var assign = new ExpressionUnit(Expression.Assign(storage, getParam.Value), getParam.Dependencies);

                emit(assign);
            }
        }

        private void compileChunks(List<Expression> output)
        {
            var buffer = new StringBuilder();
            foreach (var unit in _emitted)
            {
                compileUnit(output, buffer, unit);
            }

            flushBuffer(buffer, output);
        }

        private void compileUnit(List<Expression> output, StringBuilder outputBuffer, ExpressionUnit unit)
        {
            output.AddRange(unit.Dependencies);

            if (unit.WriteToOutput)
            {
                var constant = unit.Value as ConstantExpression;
                if (constant != null)
                {
                    outputBuffer.Append(constant.Value);
                    return;
                }

                flushBuffer(outputBuffer, output);
                output.Add(writeBytes(unit.Value));
            }
            else
            {
                flushBuffer(outputBuffer, output);
                output.Add(unit.Value);
            }
        }

        private void flushBuffer(StringBuilder data, List<Expression> compiled)
        {
            if (data.Length == 0)
            {
                //nothing to flush
                return;
            }
            var output = data.ToString();
            var bytes = Expression.Constant(Encoding.UTF8.GetBytes(output));
            var write = Expression.Call(ResponseParameter, WriteMethod, bytes);
            compiled.Add(write);

            data.Clear();
        }

        private Expression writeBytes(Expression data)
        {
            var bytesProvider = Expression.Call(null, ConvertBytesMethod, data);
            return Expression.Call(ResponseParameter, WriteMethod, bytesProvider);
        }

        #endregion

        #region Private utilites

        private void resultConcat(Instruction emitingInstruction, params object[] chunks)
        {
            foreach (var chunk in chunks)
            {
                if (chunk == null)
                    continue;

                var expr = chunk as ExpressionUnit;

                if (expr == null)
                    expr = new ExpressionUnit(Expression.Constant(chunk));

                emitWrite(expr);
            }
        }

        private ExpressionUnit attributesToString(ExpressionUnit attributesContainer, bool precompute)
        {
            return CallMethod(precompute, "AttributesToString", attributesContainer);
        }

        private void emitWrite(ExpressionUnit value)
        {
            var unit = new ExpressionUnit(value.Value, true, value.Dependencies.ToArray());
            emit(unit);
        }

        private void emit(ExpressionUnit value)
        {
            _emitted.Add(value);
        }

        private ExpressionUnit unit(Expression expression)
        {
            return new ExpressionUnit(expression);
        }

        private ExpressionUnit precomputeExpression(ExpressionUnit staticExpression)
        {
            //TODO run dependencies
            var value = staticExpression.Value;

            if (value.Type.IsValueType)
            {
                value = Expression.Convert(value, typeof(object));
            }

            var statements = new List<Expression>(staticExpression.Dependencies);
            var returnLabel = Expression.Label(typeof(object));
            statements.Add(Expression.Return(returnLabel, value));
            statements.Add(Expression.Label(returnLabel, value));

            var block = Expression.Block(_temporaryVariables, statements.ToArray());
            var evaluator = Expression.Lambda<PartialEvaluator>(block).Compile();
            var precomputed = evaluator();

            return new ExpressionUnit(Expression.Constant(precomputed));
        }

        #endregion



        internal ParameterExpression CreateTemporary(Type type)
        {
            var temp = Expression.Variable(type);

            _temporaryVariables.Add(temp);

            return temp;
        }
    }
}
