using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Compiling;

namespace ServeRick.Languages.HAML.Compiling
{
    class Context
    {
        private readonly Dictionary<string, Type> _params = new Dictionary<string, Type>();

        private readonly Dictionary<string, VariableInstruction> _variables = new Dictionary<string, VariableInstruction>();

        private readonly Context _parentContext;

        internal bool HasParent { get { return _parentContext != null; } }

        internal readonly Emitter Emitter;

        internal Context(Emitter emitter)
        {
            Emitter = emitter;
        }

        private Context(Context parentContext)
        {
            Emitter = parentContext.Emitter;
            _parentContext = parentContext;
        }

        internal Type ParamType(string name)
        {
            Type result;
            if (_params.TryGetValue(name, out result))
                return result;

            if (HasParent)
                return _parentContext.ParamType(name);

            return null;
        }

        internal VariableInstruction GetVariable(string name)
        {
            VariableInstruction result;
            if (_variables.TryGetValue(name, out result))
                return result;

            if (HasParent)
                return _parentContext.GetVariable(name);

            return null;
        }

        internal void DeclareParam(string name, string typeName)
        {
            var type = ResolveType(typeName);
            Emitter.DeclareParam(name, type);
            _params.Add(name, type);
        }

        internal VariableInstruction DeclareVariable(string name, string typeName)
        {
            var type = ResolveType(typeName);
            return DeclareVariable(name, type);
        }

        internal VariableInstruction DeclareVariable(string name, Type type)
        {
            var variable = Emitter.CreateVariable(name, type);
            _variables.Add(name, variable);

            return variable;
        }

        internal Type ResolveType(string typeName)
        {
            var type = TypeFactory.ResolveType(typeName);

            if (type == null)
                throw new NotSupportedException("Unknown type: " + typeName);

            return type;
        }

        internal Context CreateSubContext()
        {
            return new Context(this);
        }
    }
}
