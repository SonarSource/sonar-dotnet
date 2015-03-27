using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
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
