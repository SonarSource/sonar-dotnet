namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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