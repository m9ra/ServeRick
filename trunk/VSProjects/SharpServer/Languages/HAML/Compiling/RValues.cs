using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using SharpServer.Compiling;

namespace SharpServer.Languages.HAML.Compiling
{
    abstract class RValue
    {
        protected readonly Context Context;

        protected Emitter E { get { return Context.Emitter; } }

        internal abstract Instruction ToInstruction();

        internal abstract Type ReturnType();

        internal RValue(Context context)
        {
            Context = context;
        }
    }

    class VariableValue : RValue
    {
        readonly VariableInstruction _variable;

        internal VariableValue(string variableName, Context context)
            : base(context)
        {
            _variable = context.GetVariable(variableName);
            if (_variable == null)
                throw new KeyNotFoundException("Variable: " + variableName);
        }

        internal override Instruction ToInstruction()
        {
            return _variable;
        }

        internal override Type ReturnType()
        {
            return _variable.ReturnType;
        }
    }

    class LambdaBlock : RValue
    {
        private readonly Instruction _blockInstruction;
        internal readonly VariableInstruction[] Parameters;

        internal LambdaBlock(Instruction blockInstruction, IEnumerable<VariableInstruction> blockParameters, Context context)
            : base(context)
        {
            _blockInstruction = blockInstruction;
            Parameters = blockParameters.ToArray();
        }

        internal override Instruction ToInstruction()
        {
            return _blockInstruction;
        }

        internal override Type ReturnType()
        {
            return typeof(void);
        }
    }

    class IntervalValue : RValue
    {
        readonly RValue _from;
        readonly RValue _to;

        internal IntervalValue(RValue from, RValue to, Context context)
            :base(context)
        {
            _from = from;
            _to = to;
        }

        internal override Instruction ToInstruction()
        {
            return E.CreateObject<HAMLInterval>(_from.ToInstruction(), _to.ToInstruction());
        }

        internal override Type ReturnType()
        {
            return typeof(IEnumerable<int>);
        }
    }

    class ForeachValue : RValue
    {
        readonly RValue _enumerated;

        readonly LambdaBlock _block;


        internal ForeachValue(RValue enumerated, LambdaBlock block, Context context)
            : base(context)
        {
            _enumerated = enumerated;
            _block = block;
        }

        internal override Instruction ToInstruction()
        {
            var enumerated = _enumerated.ToInstruction();
            var block = _block.ToInstruction();
            var enumeratedParam = _block.Parameters[0];
            var enumeratedType = enumeratedParam.VariableType;

            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(enumeratedType);
            var enumeratorVar = E.CreateVariable("Enumerator", enumeratorType);

            var getEnumerator = E.MethodCall(enumerated, "GetEnumerator");
            var assignEnumerator = E.Assign(enumeratorVar, getEnumerator);
            var moveNextCall = E.MethodCall(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));
            var current = E.MethodCall(enumeratorVar, "get_Current");
            var assignCurrent = E.Assign(enumeratedParam, current);

            var loopSequence=E.Sequence(new []{
                assignCurrent,

                block
            });


            var result = E.Sequence(new[]{
                assignEnumerator,

                E.While(moveNextCall,
                    loopSequence
                )
            });

            return result;
        }

        internal override Type ReturnType()
        {
            return typeof(void);
        }
    }

    class YieldValue : RValue
    {
        internal readonly RValue Identifier;

        internal YieldValue(RValue identifier, Context context)
            : base(context)
        {
            Identifier = identifier;
        }

        internal override Instruction ToInstruction()
        {
            var identifierInstr = Identifier == null ? null : Identifier.ToInstruction();
            return E.Yield(identifierInstr);
        }

        internal override Type ReturnType()
        {
            return typeof(void);
        }
    }

    class ResponseCallValue : RValue
    {
        internal readonly RValue[] Args;

        internal readonly string CallName;

        internal ResponseCallValue(string callName, RValue[] args, Context context)
            : base(context)
        {
            Args = args;
            CallName = callName;
        }

        internal override Instruction ToInstruction()
        {
            var argExprs = new List<Instruction>();
            foreach (var arg in Args)
            {
                argExprs.Add(arg.ToInstruction());
            }

            return E.ResponseCall(CallName, argExprs.ToArray());
        }

        internal override Type ReturnType()
        {
            throw new NotImplementedException();
        }
    }

    class ConditionValue : RValue
    {
        internal readonly RValue Value;

        internal ConditionValue(RValue value, Context context)
            : base(context)
        {
            Value = value;
        }

        internal override Instruction ToInstruction()
        {
            var valueInstruction = Value.ToInstruction();
            var valueType = valueInstruction.ReturnType;
            var defaultValue = E.DefaultValue(valueType);

            return E.Not(E.IsEqual(valueInstruction, defaultValue));
        }

        internal override Type ReturnType()
        {
            return typeof(bool);
        }
    }

    class CallValue : RValue
    {
        internal readonly string CallName;
        internal readonly RValue[] Args;

        internal CallValue(string callName, RValue[] args, Context context)
            : base(context)
        {
            CallName = callName;
            Args = args;
        }

        internal override Instruction ToInstruction()
        {
            var argExprs = new List<Instruction>();
            foreach (var arg in Args)
            {
                argExprs.Add(arg.ToInstruction());
            }

            return E.Call(CallName, argExprs.ToArray());
        }

        internal override Type ReturnType()
        {
            throw new NotImplementedException();
        }
    }

    class ParamValue : RValue
    {
        internal readonly string ParamName;
        internal ParamValue(string paramName, Context context)
            : base(context)
        {
            ParamName = paramName;
        }

        internal override Instruction ToInstruction()
        {
            return E.GetParam(ParamName);
        }

        internal override Type ReturnType()
        {
            return Context.ParamType(ParamName);
        }
    }

    class LiteralValue : RValue
    {
        internal readonly object Literal;
        internal LiteralValue(object literal, Context context)
            : base(context)
        {
            Literal = literal;
        }


        internal override Instruction ToInstruction()
        {
            return E.Constant(Literal);
        }

        internal override Type ReturnType()
        {
            return Literal.GetType();
        }
    }

    class HashValue : RValue
    {
        internal readonly IEnumerable<RValue> Pairs;

        internal HashValue(IEnumerable<RValue> pairs, Context context)
            : base(context)
        {
            Pairs = pairs;
        }

        internal override Instruction ToInstruction()
        {
            var pairInstructions = new List<Instruction>();

            foreach (var pair in Pairs)
            {
                pairInstructions.Add(pair.ToInstruction());
            }

            return E.Container(pairInstructions);
        }

        internal override Type ReturnType()
        {
            throw new NotImplementedException();
        }
    }

    class PairValue : RValue
    {
        internal readonly RValue Key;

        internal readonly RValue Value;

        internal PairValue(RValue key, RValue value, Context context)
            : base(context)
        {
            Key = key;
            Value = value;
        }

        internal override Instruction ToInstruction()
        {
            return E.Pair(Key.ToInstruction(), Value.ToInstruction());
        }

        internal override Type ReturnType()
        {
            return typeof(KeyValuePair<,>).MakeGenericType(Key.ReturnType(), Value.ReturnType());
        }
    }

    class TagValue : RValue
    {
        internal readonly RValue TagName;

        internal readonly RValue ExplicitClass;

        internal readonly RValue ExplicitID;

        internal readonly RValue Attributes;

        Instruction _content;

        internal TagValue(RValue tag, RValue explClass, RValue explId, RValue attributes, Context context)
            : base(context)
        {
            TagName = tag;
            ExplicitClass = explClass;
            ExplicitID = explId;
            Attributes = attributes;
        }

        internal void SetContent(Instruction content)
        {
            _content = content;
        }

        internal override Instruction ToInstruction()
        {
            var tag = E.Tag(TagName.ToInstruction());
            var attributes = createAttributes();

            tag.SetAttributes(attributes);
            tag.SetContent(_content);

            return tag;
        }

        private Instruction createAttributes()
        {
            Instruction attributes;
            var idConstant = E.Constant("id");
            var classConstant = E.Constant("class");

            if (Attributes == null)
            {
                var pairs = new List<Instruction>();

                if (ExplicitID != null)
                    pairs.Add(E.Pair(idConstant, ExplicitID.ToInstruction()));

                if (ExplicitClass != null)
                    pairs.Add(E.Pair(classConstant, ExplicitClass.ToInstruction()));

                if (pairs.Count == 0)
                    return null;

                attributes = E.Container(pairs);

            }
            else
            {
                attributes = Attributes.ToInstruction();
                if (ExplicitID != null)
                    attributes = E.SetValue(attributes, idConstant, ExplicitID.ToInstruction());

                if (ExplicitClass != null)
                    attributes = E.SetValue(attributes, classConstant, ExplicitClass.ToInstruction());
            }

            return attributes;
        }

        internal override Type ReturnType()
        {
            return typeof(void);
        }
    }
}
