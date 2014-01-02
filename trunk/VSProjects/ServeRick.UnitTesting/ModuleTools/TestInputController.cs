using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Modules.Input;

namespace ServeRick.UnitTesting.ModuleTools
{
    class TestInputController : MultiPartInput
    {
        internal Dispositions LastDispositions { get; private set; }
        internal TestPartStream LastStream { get; private set; }


        internal TestInputController(string boundary)
            : base(boundary)
        {
        }

        internal void AcceptData(byte[] data)
        {
            acceptData(data, 0, data.Length);
        }

        protected override PartStream reportPart(Dispositions dispositions)
        {
            LastDispositions = dispositions;
            LastStream = new TestPartStream();

            return LastStream;
        }
    }
}
