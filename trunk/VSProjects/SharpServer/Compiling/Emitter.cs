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
        private readonly Dictionary<string, ParamDeclaration> _declarations = new Dictionary<string, ParamDeclaration>();

        private readonly List<VariableInstruction> _variables = new List<VariableInstruction>();

        private readonly LinkedList<Instruction> _emitted = new LinkedList<Instruction>();

        private readonly WebMethods _helpers;

        public string ErrorMessage { get; private set; }

        public IEnumerable<ParamDeclaration> Parameters { get { return _declarations.Values; } }

        public bool HasError { get { return ErrorMessage != null; } }

        internal Emitter(WebMethods helpers)
        {
            _helpers = helpers;
        }

        public void ReportParseError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        #region API for declarations

        public Type ResolveType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                throw new NotSupportedException("Unknow type: " + typeName);

            return type;
        }

        internal ParamInstruction DeclareParam(string name, Type paramType)
        {
            var declaration = new ParamDeclaration(name, paramType);

            _declarations.Add(declaration.Name, declaration);

            return new ParamInstruction(declaration);
        }

        #endregion

        #region API for emitting statements

        /// <summary>
        /// Emits write into response stream on value specified by instruction
        /// </summary>
        /// <param name="expression">Expression which return value will be send to output</param>
        public void Emit(Instruction instruction)
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
            var methodInfo = _helpers.GetMethod(methodName);
            return new CallInstruction(methodInfo, args);
        }


        public Instruction CreateObject<ObjectType>(params Instruction[] ctorArgs)
        {
            var type=typeof(ObjectType);
            var argTypes=from arg in ctorArgs select arg.ReturnType;
            var ctorInfo = type.GetConstructor(argTypes.ToArray());

            if (ctorInfo == null)
                throw new KeyNotFoundException("Constructor hasn't been found");

            return new ConstructorInstruction(ctorInfo, ctorArgs);
        }


        public Instruction MethodCall(Instruction thisObj, string methodName, params Instruction[] args)
        {
            //TODO correct resolving
            var methodInfo = thisObj.ReturnType.GetMethod(methodName,BindingFlags.Instance | BindingFlags.Public |BindingFlags.FlattenHierarchy);

            return MethodCall(thisObj, methodInfo, args);
        }

        public Instruction MethodCall(Instruction thisObj, MethodInfo methodInfo , params Instruction[] args)
        {
            if (methodInfo == null)
                throw new NotSupportedException("Cannot find requested method");

            return new MethodCallInstruction(thisObj, methodInfo, args);
        }

        public Instruction ResponseCall(string methodName, Instruction[] args)
        {
            var thisObj = new ResponseInstruction();
            var method = getResponseMethod(methodName);
            return new MethodCallInstruction(thisObj, method.Info, args);
        }

        /// <summary>
        /// Emit instruction representation of value
        /// </summary>
        /// <param name="value">Emitted value</param>
        /// <param name="explicitType">Explicit type is used when type cannot </param>
        /// <returns>Emitted constant</returns>
        public Instruction Constant(object value, Type explicitType = null)
        {
            return new ConstantInstruction(value, explicitType);
        }

        public Instruction GetParam(string name)
        {
            var declaration = _declarations[name];

            return new ParamInstruction(declaration);
        }

        internal Instruction Sequence(IEnumerable<Instruction> stringExpressions)
        {
            return new SequenceInstruction(stringExpressions);
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

        internal Instruction WriteInstruction(Instruction output)
        {
            return new WriteInstruction(output);
        }

        internal Instruction If(Instruction condition, Instruction ifBranch, Instruction elseBranch)
        {
            return new IfInstruction(condition, ifBranch, elseBranch);
        }

        internal Instruction SetValue(Instruction container, Instruction key, Instruction value)
        {
            var setValue = getCompilerMethod("SetValue");
            return new CallInstruction(setValue, container, key, value);
        }

        internal Instruction Yield(Instruction identifierInstr)
        {
            if (identifierInstr == null)
                //default yield identifier
                identifierInstr = Constant("");

            var yield = getCompilerMethod("Yield");
            return new CallInstruction(yield, new ResponseInstruction(), identifierInstr);
        }
        
        internal Instruction While(Instruction condition, Instruction loopBlock)
        {
            return new WhileInstruction(condition, loopBlock);
        }

        internal Instruction DefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                var defaultInstance = Activator.CreateInstance(type);
                return Constant(defaultInstance);
            }

            return Constant(null, type);
        }

        internal VariableInstruction CreateVariable(string nameHint, Type variableType)
        {
            var variable=new VariableInstruction(nameHint, variableType);

            _variables.Add(variable);

            return variable;
        }

        internal Instruction Assign(VariableInstruction variable, Instruction assignedValue)
        {
            return new AssignInstruction(variable, assignedValue);
        }

        internal Instruction IsEqual(Instruction value1, Instruction value2)
        {
            var isEqual = getCompilerMethod("IsEqual");
            return new CallInstruction(isEqual, value1, value2);
        }

        internal Instruction Not(Instruction value)
        {
            var not = getCompilerMethod("Not");
            return new CallInstruction(not, value);
        }

        #endregion

        /// <summary>
        /// Get response handler compiled from emitted statements
        /// </summary>
        /// <returns>Compiled response handler</returns>
        internal Instruction GetEmittedResult()
        {
            return new SequenceInstruction(_emitted);
        }

        private void emit(Instruction instruction)
        {
            _emitted.AddLast(instruction);
        }

        private WebMethod getCompilerMethod(string methodName)
        {
            return ResponseHandlerProvider.CompilerHelpers.GetMethod(methodName);
        }

        private WebMethod getResponseMethod(string methodName)
        {
            throw new NotImplementedException();
        }
    }
}
