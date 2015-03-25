using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class ForLoopCounterConditionTest
    {
        [TestMethod]
        public void ForLoopCounterCondition()
        {
            Verifier.Verify(@"TestCases\ForLoopCounterCondition.cs", new ForLoopCounterCondition());
        }
    }
}
