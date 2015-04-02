using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
