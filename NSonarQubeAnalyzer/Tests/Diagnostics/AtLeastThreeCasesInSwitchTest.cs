using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
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
