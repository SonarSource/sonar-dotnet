using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class UnnecessaryBooleanLiteralTest
    {
        [TestMethod]
        public void UnnecessaryBooleanLiteral()
        {
            Verifier.Verify(@"TestCases\UnnecessaryBooleanLiteral.cs", new UnnecessaryBooleanLiteral());
        }
    }
}
