using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class ForLoopCounterChangedTest
    {
        [TestMethod]
        public void ForLoopCounterChanged()
        {
            Verifier.Verify(@"TestCases\ForLoopCounterChanged.cs", new ForLoopCounterChanged());
        }
    }
}
