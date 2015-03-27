using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class UseCurlyBracesTest
    {
        [TestMethod]
        public void UseCurlyBraces()
        {
            Verifier.Verify(@"TestCases\UseCurlyBraces.cs", new UseCurlyBraces());
        }
    }
}
