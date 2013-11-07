using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServeRick.UnitTesting.ModuleTools;

namespace ServeRick.UnitTesting
{
    [TestClass]
    public class MultiPart_Tests
    {
        [TestMethod]
        public void MultiPart_BoundaryStart()
        {
            @"--boundary--boundary"
            .Boundary("boundary")
            .AssertStart(0 - 2); //first boundary doesnt have first chars visible
        }

        [TestMethod]
        public void MultiPart_BoundaryShiftedStart()
        {
            "01234\r\n--boundary--boundary"
            .Boundary("boundary")
            .AssertStart(5);
        }

        /// <summary>
        /// TODO: needs algorithm that can cope with back edges
        /// </summary>
        //[TestMethod]
        public void MultiPart_BackEdgeBoundary()
        {
            @"--a--a--b"
            .Boundary("a--b")
            .AssertStart(3);
        }

        [TestMethod]
        public void MultiPart_ContentParsing()
        {
            @"--boundary
Content-Disposition: form-data; name=""paramname""; filename=""foo.txt""
Content-Type: text/plain

abcdef
--boundary--"
                .Boundary("boundary")
                .AssertContent("abcdef");
        }

    }
}
