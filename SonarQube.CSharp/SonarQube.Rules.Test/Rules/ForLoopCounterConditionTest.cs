using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
