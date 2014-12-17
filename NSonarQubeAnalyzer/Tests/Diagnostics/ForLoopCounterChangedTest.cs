namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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