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
    /// <summary>
    /// Compile instructions into ResponseHandler writing Raw http (without headers)
    /// </summary>
    class HTMLCompiler : InstructionVisitor
    {
        private static readonly Type ResponseType = typeof(Response);
        private static readonly MethodInfo WriteMethod = ResponseType.GetMethod("Write");

        private static readonly MethodInfo ConvertBytesMethod = typeof(HTMLCompiler).GetMethod("ConvertBytes", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly ParameterExpression _param = Expression.Parameter(ResponseType);
        private readonly List<Expression> _chunks = new List<Expression>();
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

        #region Instruction visitor overrides

        public override void VisitInstruction(Instruction x)
        {
            throw new NotSupportedException("Unsupported instruction visited");
        }

        public override void VisitConcat(ConcatInstruction x)
        {
            foreach (var chunk in x.Chunks)
            {
                var value = HTMLValues.GetValue(chunk);
                emitWrite(value);
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
            return Expression.Lambda<ResponseHandler>(output, _param).Compile();
        }

        private List<Expression> compileChunks()
        {
            var compiled = new List<Expression>();

            var buffer = new StringBuilder();
            foreach (var chunk in _chunks)
            {
                var constant = chunk as ConstantExpression;
                if (constant != null)
                {
                    buffer.Append(constant.Value);
                    continue;
                }

                flushBuffer(buffer, compiled);
                compiled.Add(writeBytes(chunk));
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
            var write=Expression.Call(_param, WriteMethod, bytes);
            compiled.Add(write);

            data.Clear();
        }

        private Expression writeBytes(Expression data)
        {
            var bytesProvider = Expression.Call(null, ConvertBytesMethod, data);
            return Expression.Call(_param, WriteMethod, bytesProvider);
        }

        #endregion

        #region Private utilites

        private void emitWrite(Expression statement)
        {
            _chunks.Add(statement);
        }

        #endregion
    }
}
