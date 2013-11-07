using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServeRick.Modules.Input;

namespace ServeRick.UnitTesting.ModuleTools
{
    class MultiPartTest
    {
        readonly Boundary _boundary;
        readonly TestInputController _controller;

        public MultiPartTest(string input, string delimiter)
        {
            _boundary = new Boundary(delimiter);
            _controller = new TestInputController(delimiter);

            var inputData = Converter.GetInputBytes(input);
            _boundary.AcceptNext(inputData, 0, inputData.Length);
            _controller.AcceptData(inputData);
        }

        public MultiPartTest AssertStart(int expectedStart)
        {
            Assert.IsTrue(_boundary.IsComplete, "Boundary is expected to be complete");
            Assert.AreEqual(expectedStart, _boundary.LocalStartOffset, "Boundary start is misplaced");

            return this;
        }

        internal void AssertContent(string expectedContent)
        {
            var part=_controller.LastStream;

            Assert.IsTrue(part.IsComplete, "Part stream is expected to be complete");
            Assert.AreEqual(expectedContent, part.Data.ToString(), "Incorrect part content");
        }
    }
}
