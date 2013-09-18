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
        private readonly List<EmittedItem> _emitted = new List<EmittedItem>();
        private readonly StringBuilder _staticWrites = new StringBuilder();

        /// <summary>
        /// Run compilation of given instruction (All write instructions are sended to output in pure HTML)
        /// </summary>
        /// <param name="instruction">Instruction to be compiled</param>
        /// <returns>Compiled response handler</returns>
        public static ResponseHandler Compile(Instruction instruction)
        {
            var compiler = new HTMLCompiler();
            instruction.VisitMe(compiler);

            return compiler.compile();
        }

        public Expression GetValue(Instruction instruction)
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

        public Expression CallMethod(bool precompute,string name, params Expression[] args)
        {
            return CallMethod(precompute,name, (IEnumerable<Expression>)args);
        }

        public Expression CallMethod(bool precompute, string name, IEnumerable<Expression> args)
        {
            var method = ResponseHandlerProvider.CompilerHelpers.GetMethod(name);
            return CallMethod(precompute,method.Info, args);
        }

        public Expression CallMethod(bool precompute, MethodInfo methodInfo, IEnumerable<Expression> args)
        {
            var matcher = new MethodMatcher(methodInfo, args);
            var call=matcher.CreateCall();

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

            var stringyAttributes = attributes == null ? Expression.Constant("") : attributesToString(attributes,x.Attributes.IsStatic());

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


        #endregion

        #region Services for compiler

        internal static byte[] ConvertBytes(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        private ResponseHandler compile()
        {
            var compiled = compileChunks();
            var output = Expression.Block(compiled);
            return Expression.Lambda<ResponseHandler>(output, ResponseParameter).Compile();
        }

        private List<Expression> compileChunks()
        {
            var compiled = new List<Expression>();

            var buffer = new StringBuilder();
            foreach (var chunk in _emitted)
            {
                if (chunk.WriteToOutput)
                {
                    var constant = chunk.Expression as ConstantExpression;
                    if (constant != null)
                    {
                        buffer.Append(constant.Value);
                        continue;
                    }

                    flushBuffer(buffer, compiled);
                    compiled.Add(writeBytes(chunk.Expression));
                }
                else
                {
                    flushBuffer(buffer, compiled);
                    compiled.Add(chunk.Expression);
                }
            }

            flushBuffer(buffer, compiled);
            return compiled;
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

                var expr = chunk as Expression;

                if (expr == null)
                    expr = Expression.Constant(chunk);

                emitWrite(expr);
            }
        }

        private Expression attributesToString(Expression attributesContainer,bool precompute)
        {
            return CallMethod(precompute,"AttributesToString", attributesContainer);
        }

        private void emitWrite(Expression statement)
        {
            _emitted.Add(new EmittedItem(statement, true));
        }

        private void emit(Expression statement)
        {
            _emitted.Add(new EmittedItem(statement, false));
        }

        private static Expression precomputeExpression(Expression staticExpression)
        {
            var evaluator = Expression.Lambda<PartialEvaluator>(staticExpression).Compile();
            var precomputed = evaluator();

            return Expression.Constant(precomputed);
        }

        #endregion
    }
}
