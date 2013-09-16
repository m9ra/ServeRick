using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

namespace SharpServer.Compiling
{
    class MethodMatcher
    {
        private MethodInfo _method;

        private Expression[] _args;

        private readonly Dictionary<Type, Type> _assignings = new Dictionary<Type, Type>();

        internal MethodMatcher(MethodInfo method, IEnumerable<Expression> args)
        {
            _method = method;
            _args = args.ToArray();
        }

        internal Expression CreateCall()
        {
            matchTypeParameters();

            return Expression.Call(null, _method, _args);
        }

        private void matchTypeParameters()
        {
            if (!_method.ContainsGenericParameters)
            {
                //there is nothing to convert
                //note matched methods doesn't contains generic parameters              
                return;
            }

            foreach (var typeParam in _method.GetGenericArguments())
            {
                assign(typeParam, null);
            }

            var methodParams = _method.GetParameters();

            for (int i = 0; i < methodParams.Length; ++i)
            {
                //TODO proper args matching
                var arg = _args[i];
                var argType = arg.Type;
                var param = methodParams[i];
                var paramType = param.ParameterType;

                if (isEnumerable(paramType) && !isEnumerable(argType))
                {
                    //needs implicit conversion
                    var isLastParam = i + 1 == methodParams.Length;
                    if (!isLastParam)
                    {
                        throw new NotSupportedException("Cannot make implicit conversion on non-last parameter");
                    }

                    //set converted argument
                    var argValues = _args.Skip(i);
                    _args[i] = Expression.NewArrayInit(argType, argValues.ToArray());
                    Array.Resize(ref _args, i + 1);

                    //match type
                    match(paramType, typeof(IEnumerable<>).MakeGenericType(argType));
                }
                else
                {
                    //direct matching
                    match(paramType, arg.Type);
                }
            }

            foreach (var assign in _assignings)
            {
                if (assign.Value == null)
                {
                    throw new NotSupportedException("Cannot match type parameters");
                }
            }

            _method = _method.MakeGenericMethod(_assignings.Values.ToArray());
        }

        private void match(Type t1, Type t2)
        {
            if (_assignings.ContainsKey(t1))
            {
                //we have found matching
                _assignings[t1] = t2;
                return;
            }

            if (!t1.IsGenericType || !t2.IsGenericType)
            {
                return;
            }

            var t1Generic = t1.GetGenericTypeDefinition();
            var t2Generic = t2.GetGenericTypeDefinition();

            if (t1Generic != t2Generic)
            {
                //there is no matching between types
                return;
            }

            var t1Params = t1.GetGenericArguments();
            var t2Params = t2.GetGenericArguments();

            //because of sam generic types there has to be same count of parameters
            for (int i = 0; i < t1Params.Length; ++i)
            {
                match(t1Params[i], t2Params[i]);
            }
        }

        private bool isEnumerable(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            var genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(IEnumerable<>))
            {
                return true;
            }

            return false;
        }

        private void assign(Type typeParam, Type type)
        {
            _assignings[typeParam] = type;
        }
    }
}
