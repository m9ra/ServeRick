using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ServeRick.UnitTesting.NetworkTools;

namespace ServeRick.UnitTesting
{
    [TestClass]
    public class HeaderParsing_Tests
    {
        [TestMethod]
        public void GET_ParseHeader()
        {
            @"GET / HTTP/1.1
Host: test.cz
Connection: keep-alive
Cache-Control: max-age=0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.62 Safari/537.36
Accept-Encoding: gzip,deflate,sdch
Accept-Language: cs-CZ,cs;q=0.8
Cookie: quick_list_box=show
If-None-Match: ""405800124""
If-Modified-Since: Tue, 03 Sep 2013 16:17:21 GMT

"
                .H("Connection", "keep-alive")
                .H("cookie", "quick_list_box=show")
                .H("IF-MODIFIED-SINCE", "Tue, 03 Sep 2013 16:17:21 GMT")
                .H("hOsT", "test.cz")
 ;
        }
    }
}
