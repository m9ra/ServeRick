using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpServer.Networking;

namespace SharpServer.UnitTesting.NetworkTools
{
    static class Extensions
    {
        internal static HeaderParsingCase H(this string requestData, string headerName, string headerValue )
        {
            var parser=new HttpRequestParser();

            var requestBytes=Encoding.ASCII.GetBytes(requestData);
            parser.AppendData(requestBytes,requestBytes.Length);
            var request = parser.GetRequest();

            Assert.IsFalse(request.ContainsError,"Spurious error detected in error free request");
            Assert.IsTrue(request.IsHeadComplete,"Request head parsing hasn't been completed");
            Assert.IsTrue(request.IsComplete,"Request parsing hasn't been completed");

            
            

            
            var testCase = new HeaderParsingCase(requestData, request);

            return testCase.H(headerName,headerValue);
        }
    }
}
