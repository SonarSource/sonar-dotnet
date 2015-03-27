using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
