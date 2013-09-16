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
    /// Emitter of response handlers
    /// </summary>
    public class Emitter
    {
        private readonly LinkedList<Instruction> _emitted = new LinkedList<Instruction>();

        private readonly WebMethods _methods;

        internal Emitter(WebMethods methods)
        {
            _methods = methods;
        }

        #region API for emitting statements

        /// <summary>
        /// Emits write into response stream on value specified by instruction
        /// </summary>
        /// <param name="expression">Expression which return value will be send to output</param>
        public void Write(Instruction instruction)
        {
            _emitted.AddLast(instruction);
        }
        
        /// <summary>
        /// Create call of given method with given arguments
        /// </summary>
        /// <param name="methodName">Name of method that will be called</param>
        /// <param name="args">Arguments for call</param>
        /// <returns>Created call</returns>
        public Instruction Call(string methodName, Instruction[] args)
        {
            var methodInfo = getMethod(methodName);
            return new CallInstruction(methodInfo, args);
        }

        /// <summary>
        /// Emit instruction representation of value
        /// </summary>
        /// <param name="value">Emitted value</param>
        /// <returns>Emitted constant</returns>
        public Instruction Constant(object value)
        {
            return new ConstantInstruction(value);
        }

        #endregion

        /// <summary>
        /// Get response handler compiled from emitted statements
        /// </summary>
        /// <returns>Compiled response handler</returns>
        internal Instruction GetEmittedResult()
        {
            return new ConcatInstruction(_emitted);
        }

        internal Instruction Concat(IEnumerable<Instruction> stringExpressions)
        {
            return new ConcatInstruction(stringExpressions);
        }

        internal TagInstruction Tag(Instruction tagName)
        {
            return new TagInstruction(tagName);
        }

        internal Instruction Pair(Instruction key, Instruction value)
        {
            return new PairInstruction(key, value);
        }

        internal Instruction Container(List<Instruction> pairs)
        {
            return new ContainerInstruction(pairs);
        }

        internal Instruction SetValue(Instruction container, Instruction key, Instruction value)
        {
            var setValue = getMethod("SetValue");
            return new CallInstruction(setValue, container, key, value);
        }

        private void emit(Instruction instruction)
        {
            _emitted.AddLast(instruction);
        }

        private WebMethod getMethod(string methodName)
        {
            return _methods.GetMethod(methodName);
        }
    }
}
