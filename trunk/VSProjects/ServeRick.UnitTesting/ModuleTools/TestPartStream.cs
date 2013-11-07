using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Modules.Input;

namespace ServeRick.UnitTesting.ModuleTools
{
    class TestPartStream : PartStream
    {
        internal readonly StringBuilder Data = new StringBuilder();
        internal bool IsComplete { get; private set; }

        internal TestPartStream()
            : base(ulong.MaxValue)
        {
        }

        protected override bool write(byte[] data, int dataStart, int dataLength)
        {
            var converted = Encoding.ASCII.GetString(data, dataStart, dataLength);
            Data.Append(converted);

            return true;
        }

        protected override void Completed()
        {
            IsComplete = true;
        }
    }
}
