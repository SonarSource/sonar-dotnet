using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class IfConditionalAlwaysTrueOrFalseTest
    {
        [TestMethod]
        public void IfConditionalAlwaysTrueOrFalse()
        {
            Verifier.Verify(@"TestCases\IfConditionalAlwaysTrueOrFalse.cs", new IfConditionalAlwaysTrueOrFalse());
        }
    }
}
