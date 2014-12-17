namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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