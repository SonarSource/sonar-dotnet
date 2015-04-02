using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
