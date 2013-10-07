using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpServer.UnitTesting.CompilingTools;

namespace SharpServer.UnitTesting
{
    [TestClass]
    public class Scss_Tests
    {
        [TestMethod]
        public void SCSS_SimpleBlock()
        {
            var block = @"
a:hover,#id:visited,p{
 margin: 10px 0;
}
";
            //output block will be same
            block.AssertSCSS(block);
        }

        [TestMethod]
        public void SCSS_VariableUsage()
        {
            @"
$variable: #abcdef;
a{
    color: $variable;
}
".AssertSCSS(@"
a{
    color: #abcdef;
}
");
 
        }
    }
}
