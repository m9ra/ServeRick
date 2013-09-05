using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

namespace SharpServer.Compiling
{
    /// <summary>
    /// Emitter of response handlers
    /// </summary>
    public class Emitter
    {
        private static readonly Type ResponseType = typeof(Response);
        private static readonly MethodInfo WriteMethod = ResponseType.GetMethod("Write");

        private static readonly MethodInfo ConvertBytesMethod = typeof(Emitter).GetMethod("ConvertBytes",BindingFlags.Static | BindingFlags.NonPublic);
        
        private readonly ParameterExpression _param = Expression.Parameter(ResponseType);
        private readonly List<Expression> _statements = new List<Expression>();
        private readonly StringBuilder _staticWrites = new StringBuilder();
        

        private static readonly Dictionary<string, MethodInfo> _helperMethods = new Dictionary<string, MethodInfo>();



        static Emitter()
        {
            var helpersType = typeof(Helpers);
            foreach (var method in helpersType.GetMethods())
            {
                _helperMethods[method.Name] = method;
            }
        }

        internal static byte[] ConvertBytes(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }


        #region API for emitting statements

        /// <summary>
        /// Emits static write into response stream
        /// </summary>
        /// <param name="data">Static data written to stream</param>
        public void StaticWrite(string data)
        {               
            _staticWrites.Append(data);
        }

        /// <summary>
        /// Emits write into response stream on value returned by expression
        /// </summary>
        /// <param name="expression">Expression which output will be send to output</param>
        public void Write(Expression expression)
        {
            var bytesProvider = Expression.Call(null, ConvertBytesMethod, expression);

            emitWrite(bytesProvider);
        }

        public Expression Call(string methodName, Expression[] args)
        {
            var methodInfo = getMethod(methodName);
            return Expression.Call(null, methodInfo, args);
        }

        public Expression Constant(object value)
        {
            return Expression.Constant(value);
        }
        #endregion

        /// <summary>
        /// Get response handler compiled from emitted statements
        /// </summary>
        /// <returns>Compiled response handler</returns>
        internal ResponseHandler GetResult()
        {
            finalizeEmitting();
            var viewBlock = Expression.Block(_statements.ToArray());
            return Expression.Lambda<ResponseHandler>(viewBlock, _param).Compile();
        }

        internal void finalizeEmitting()
        {
            emitStaticWrites();
        }

        #region Private utilites

        private void emit(Expression statement)
        {
            _statements.Add(statement);
        }

        private void emitWrite(Expression data)
        {
            emitStaticWrites();

            var write = Expression.Call(_param, WriteMethod, data);            
            emit(write);
        }

        private void emitStaticWrites()
        {
            if (_staticWrites.Length == 0)
                return;

            var data = ConvertBytes(_staticWrites.ToString());

            var dataHTML = Expression.Constant(data);
            _staticWrites.Clear();

            emitWrite(dataHTML);
        }

        private MethodInfo getMethod(string name)
        {            
            return _helperMethods[name];
        }

        #endregion
    }
}
