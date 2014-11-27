using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class TooManyLabelsInSwitchTest
    {
        [TestMethod]
        public void TooManyLabelsInSwitch()
        {
            var diagnostic = new TooManyLabelsInSwitch();
            diagnostic.Maximum = 2;
            Verifier.Verify(@"TestCases\TooManyLabelsInSwitch.cs", diagnostic);
        }
    }
}
