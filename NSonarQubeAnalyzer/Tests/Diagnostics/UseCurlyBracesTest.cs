using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
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
