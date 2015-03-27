using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class RightCurlyBraceStartsLineTest
    {
        [TestMethod]
        public void RightCurlyBraceStartsLine()
        {
            Verifier.Verify(@"TestCases\RightCurlyBraceStartsLine.cs", new RightCurlyBraceStartsLine());
        }
    }
}
