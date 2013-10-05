using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpServer.Compiling;

using SharpServer.UnitTesting.WebTools;

namespace SharpServer.UnitTesting.CompilingTools
{
    static class Extensions
    {
        internal static void AssertHAML(this string hamlInput, string expectedHtmlOutput)
        {
            assertCompiled("haml", hamlInput, expectedHtmlOutput);
        }

        internal static void AssertSCSS(this string scssInput, string expectedCssOutput)
        {
            assertCompiled("scss", scssInput, expectedCssOutput);
        }

        private static void assertCompiled(string language, string source, string expectedOutput)
        {
            var actualOutput = getCompiled(language, source);
            var normalizedExpected = normalizeText(expectedOutput);
            var normalizedActual = normalizeText(actualOutput);
            Assert.AreEqual(normalizedExpected, normalizedActual);
        }

        private static string getCompiled(string language, string source)
        {
            ServerEnvironment.LoadToolchains();
            var testWeb = new TestWebApp();

            var handler = testWeb.HandlerProvider.Compile(language, source);
            var testResponse = new TestResponse();
            handler(testResponse);

            return testResponse.WrittenData();
        }

        private static string normalizeText(string html)
        {
            return html.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t","");
        }
    }
}
