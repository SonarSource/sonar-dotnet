namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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