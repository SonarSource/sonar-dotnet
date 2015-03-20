using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    [TestClass]
    public class IdenticalExpressionsInBinaryOpTest
    {
        [TestMethod]
        public void IdenticalExpressionsInBinaryOp()
        {
            Verifier.Verify(@"TestCases\IdenticalExpressionsInBinaryOp.cs", new IdenticalExpressionsInBinaryOp());
        }
    }
}
