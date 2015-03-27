using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class AtLeastThreeCasesInSwitchTest
    {
        [TestMethod]
        public void AtLeastThreeCasesInSwitch()
        {
            Verifier.Verify(@"TestCases\AtLeastThreeCasesInSwitch.cs", new AtLeastThreeCasesInSwitch());
        }
    }
}
