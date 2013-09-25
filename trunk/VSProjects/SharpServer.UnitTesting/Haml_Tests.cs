using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpServer.UnitTesting.CompilingTools;

namespace SharpServer.UnitTesting
{
    [TestClass]
    public class Haml_Tests
    {
        [TestMethod]
        public void HAML_EmtpyElement()
        {
            @"%a"
            .AssertHAML("<a/>");
        }

        [TestMethod]
        public void HAML_ContentInline()
        {
            @"%a hello"
            .AssertHAML("<a>hello</a>");
        }

        [TestMethod]
        public void HAML_NestedElements()
        {
            @"
%a 
 %b
 %c
%d
 %e
  %f
".AssertHAML(@"<a><b/><c/></a><d><e><f/></e></d>");
        }


        [TestMethod]
        public void HAML_ChainedContent()
        {
            @"
%a 
 %b
  %c
 Test1
 Test2
 %d
 Test3
".AssertHAML(@"<a><b><c/></b>Test1Test2<d/>Test3</a>");

        }

        [TestMethod]
        public void HAML_ImplicitTag()
        {
            @".test"
            .AssertHAML("<div class=\"test\"/>");
        }

        [TestMethod]
        public void HAML_ExplicitAttributes()
        {
            @"#idTest.classTest"
            .AssertHAML("<div id=\"idTest\" class=\"classTest\"/>");
        }

        [TestMethod]
        public void HAML_ContainerAttributes()
        {
            @"%img{:src=>""image.png"", :alt=>""Test""}"
            .AssertHAML("<img src=\"image.png\" alt=\"Test\"/>");
        }

        [TestMethod]
        public void HAML_Doctype()
        {
            @"
!!!
%a
"
            .AssertHAML("<!DOCTYPE html><a/>");
        }
    }
}
