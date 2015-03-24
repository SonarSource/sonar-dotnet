using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
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
