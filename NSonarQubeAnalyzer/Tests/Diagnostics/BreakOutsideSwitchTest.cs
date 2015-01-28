namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

    [TestClass]
    public class BreakOutsideSwitchTest
    {
        [TestMethod]
        public void BreakOutsideSwitch()
        {
            Verifier.Verify(@"TestCases\BreakOutsideSwitch.cs", new BreakOutsideSwitch());
        }
    }
}