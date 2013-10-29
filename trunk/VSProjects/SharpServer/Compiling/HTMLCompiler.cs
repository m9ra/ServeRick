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
    class HTMLCompiler
    {
        private static readonly Type ResponseType = typeof(Response);
        internal readonly ParameterExpression ResponseParameter = Expression.Parameter(ResponseType, "response");

        private static readonly MethodInfo WriteMethod = ResponseType.GetMethod("Write");
        private static readonly MethodInfo ConvertBytesMethod = typeof(HTMLCompiler).GetMethod("ConvertBytes", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly Dictionary<VariableInstruction, ParameterExpression> _declaredVariables = new Dictionary<VariableInstruction, ParameterExpression>();
        private readonly List<ParameterExpression> _temporaryVariables = new List<ParameterExpression>();
        private readonly Dictionary<string, ParameterExpression> _paramStorages = new Dictionary<string, ParameterExpression>();

        /// <summary>
        /// Run compilation of given instruction (All write instructions are sended to output in pure HTML)
        /// </summary>
        /// <param name="root">Instruction to be compiled</param>
        /// <returns>Compiled response handler</returns>
        public static ResponseHandler Compile(Instruction root, IEnumerable<ParamDeclaration> parameters)
        {
            var compiler = new HTMLCompiler(parameters);
            return compiler.compile(root);
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

        internal Expression Param(string paramName)
        {
            return _paramStorages[paramName];
        }

        internal Expression Variable(VariableInstruction variableOccurance)
        {
            ParameterExpression variable;

            if (!_declaredVariables.TryGetValue(variableOccurance, out variable))
            {
                variable = GetTemporaryVariable(variableOccurance.VariableType);
                _declaredVariables[variableOccurance] = variable;
            }

            return variable;
        }

        public Expression Call(bool precompute, string name, params Expression[] args)
        {
            return Call(precompute, name, (IEnumerable<Expression>)args);
        }

        public Expression Call(bool precompute, string name, IEnumerable<Expression> args)
        {
            var method = ResponseHandlerProvider.CompilerHelpers.GetMethod(name);
            return Call(precompute, method.Info, args);
        }

        public Expression Call(bool precompute, MethodInfo methodInfo, IEnumerable<Expression> args)
        {
            return MethodCall(precompute, null, methodInfo, args);
        }

        public Expression MethodCall(bool precompute, Expression thisObj, MethodInfo methodInfo, IEnumerable<Expression> args)
        {
            var matcher = new MethodMatcher(methodInfo, args);
            var call = matcher.CreateCall(thisObj);

            if (precompute)
                return precomputeExpression(call);

            return call;
        }

        #region Services for compiler

        internal ParameterExpression GetTemporaryVariable(Type type)
        {
            if (type == typeof(void))
                return null;

            var temporary = Expression.Parameter(type);
            _temporaryVariables.Add(temporary);

            return temporary;
        }

        internal static byte[] ConvertBytes(string data)
        {
            if (data == null)
                return new byte[0];
            return Encoding.UTF8.GetBytes(data);
        }

        internal Expression CompileBlock(List<Expression> emitted)
        {
            /*    var output = new List<Expression>();
                var buffer = new StringBuilder();
                foreach (var chunk in emitted)
                {
                    if (chunk.WriteToOutput)
                    {
                        var constant = chunk.Expression as ConstantExpression;
                        if (constant != null)
                        {
                            buffer.Append(constant.Value);
                            continue;
                        }

                        flushBuffer(buffer, output);
                        output.Add(writeBytes(chunk.Expression));
                    }
                    else
                    {
                        flushBuffer(buffer, output);
                        output.Add(chunk.Expression);
                    }
                }

                flushBuffer(buffer, output);
                return Expression.Block(output.ToArray());*/

            if (emitted.Count==0)
            {
                return Expression.Constant(null);
            }
            return Expression.Block(emitted.ToArray());
        }

        private ResponseHandler compile(Instruction root)
        {
            var rootExpression = GetValue(root);

            var compiled = new List<Expression>();
            compileDeclarations(compiled);
            compiled.Add(rootExpression);

            if (compiled.Count == 0)
            {
                return (r) => { };
            }

            var variables = new List<ParameterExpression>();
            variables.AddRange(_paramStorages.Values);
            variables.AddRange(_temporaryVariables);
            var output = Expression.Block(variables, compiled);

            return Expression.Lambda<ResponseHandler>(output, ResponseParameter).Compile();
        }

        private void compileDeclarations(List<Expression> compiled)
        {
            foreach (var storage in _paramStorages.Values)
            {
                var nameExpr = Expression.Constant(storage.Name);

                var getParam = Call(false, "Param", ResponseParameter, nameExpr);
                getParam = Expression.Convert(getParam, storage.Type);
                var assign = Expression.Assign(storage, getParam);
                compiled.Add(assign);
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

        internal Expression WriteBytes(Expression data)
        {
            if (data.Type != typeof(string))
            {
                data = Expression.Call(data,typeof(object).GetMethod("ToString"));
            }

            var bytesProvider = Expression.Call(null, ConvertBytesMethod, data);
            return Expression.Call(ResponseParameter, WriteMethod, bytesProvider);
        }

        #endregion

        #region Private utilites

        private Expression precomputeExpression(Expression staticExpression)
        {
            var value = staticExpression;

            //value = simplify(value);


            if (value.Type == typeof(void))
            {
                return value;   
            }

            if (value.Type.IsValueType)
            {
                value = Expression.Convert(value, typeof(object));
            }

            var block = Expression.Block(_temporaryVariables, value);
            var evaluator = Expression.Lambda<PartialEvaluator>(block).Compile();
            var precomputed = evaluator();

            return Expression.Constant(precomputed);
        }

        private Expression simplify(Expression expression)
        {
            var block = expression as BlockExpression;
            if (block == null)
                return expression;

            throw new NotImplementedException();
            // foreach(var stmt in block.
        }

        #endregion
    }
}