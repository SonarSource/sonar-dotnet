using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

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
