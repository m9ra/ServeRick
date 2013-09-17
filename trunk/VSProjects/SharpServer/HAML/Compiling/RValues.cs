using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using SharpServer.Compiling;

namespace SharpServer.HAML.Compiling
{
    abstract class RValue
    {
        protected readonly Emitter E;
        internal abstract Instruction ToInstruction();

        internal RValue(Emitter emitter)
        {
            E = emitter;
        }
    }


    class CallValue : RValue
    {
        internal readonly string CallName;
        internal readonly RValue[] Args;

        internal CallValue(string callName, RValue[] args, Emitter emitter) :
            base(emitter)
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
    }

    class LiteralValue : RValue
    {
        internal readonly string Literal;
        internal LiteralValue(string literal, Emitter emitter) :
            base(emitter)
        {
            Literal = literal;
        }


        internal override Instruction ToInstruction()
        {
            return E.Constant(Literal);
        }
    }

    class HashValue : RValue
    {
        internal readonly IEnumerable<RValue> Pairs;

        internal HashValue(IEnumerable<RValue> pairs, Emitter emitter)
            : base(emitter)
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
    }

    class PairValue : RValue
    {
        internal readonly RValue Key;

        internal readonly RValue Value;

        internal PairValue(RValue key, RValue value, Emitter emitter)
            : base(emitter)
        {
            Key = key;
            Value = value;
        }

        internal override Instruction ToInstruction()
        {
            return E.Pair(Key.ToInstruction(), Value.ToInstruction());
        }
    }

    class TagValue : RValue
    {
        internal readonly RValue TagName;

        internal readonly RValue ExplicitClass;

        internal readonly RValue ExplicitID;

        internal readonly RValue Attributes;

        Instruction _content;

        internal TagValue(RValue tag, RValue explClass, RValue explId, RValue attributes, Emitter emitter)
            : base(emitter)
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
    }
}
