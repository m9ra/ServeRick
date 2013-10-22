using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling.Instructions
{
    class TagInstruction : Instruction
    {
        public readonly Instruction Name;
        /// <summary>
        /// Expects IDictionary string to string attribut to its value
        /// </summary>
        public Instruction Attributes { get; private set; }

        public Instruction Content { get; private set; }

        public override Type ReturnType
        {
            get { throw new NotImplementedException("What type tag instruction should be ?"); }
        }

        public TagInstruction(Instruction name)
        {
            Name = name;
        }
        internal override void VisitMe(InstructionVisitor visitor)
        {
            visitor.VisitTag(this);
        }

        /// <summary>
        /// Dictionary of type string string is expected
        /// </summary>
        /// <param name="attributesContainer"></param>
        internal void SetAttributes(Instruction attributesContainer)
        {
            Attributes = attributesContainer;
        }

        internal void SetContent(Instruction content)
        {
            Content = content;
        }

        internal override bool IsStatic()
        {
            return TestStatic(Name, Attributes, Content);
        }
    }
}
