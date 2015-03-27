using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
