using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
