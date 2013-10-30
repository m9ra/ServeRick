using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Networking;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServeRick.UnitTesting.NetworkTools
{
    class HeaderParsingCase
    {
        internal readonly string RequestData;
        internal readonly HttpRequest Headers;

        internal HeaderParsingCase(string requestData, HttpRequest headers)
        {
            RequestData = requestData;
            Headers = headers;
        }

        internal HeaderParsingCase H(string headerName, string expectedValue)
        {
            string actualValue;
            if (!Headers.TryGetHeader(headerName, out actualValue))
            {
                Assert.Fail("Expected header is missing");
            }
            
            Assert.AreEqual(expectedValue, actualValue, "Value of header is not correct");

            return this;
        }
    }
}
