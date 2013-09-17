using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpServer.Compiling;

namespace SharpServer.UnitTesting.HamlTools
{
    static class Extensions
    {
        internal static void AssertHAML(this string hamlInput, string expectedHtmlOutput)
        {
            ServerEnvironment.LoadToolchains();

            var handler=ResponseHandlerProvider.GetHandler("haml", hamlInput);
            var testResponse = new TestResponse();
            handler(testResponse);

            var actualOutput = testResponse.WrittenData();

            var normalizedExpected = normalizeHtml(expectedHtmlOutput);
            var normalizedActual = normalizeHtml(actualOutput);
            Assert.AreEqual(normalizedExpected,normalizedActual);
        }


        private static string normalizeHtml(string html)
        {
            return html.Replace(" ","").Replace("\n","").Replace("\r","");
        }
    }
}
