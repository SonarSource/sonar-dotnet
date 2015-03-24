using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class ConditionalStructureSameConditionTest
    {
        [TestMethod]
        public void ConditionalStructureSameCondition()
        {
            Verifier.Verify(@"TestCases\ConditionalStructureSameCondition.cs", new ConditionalStructureSameCondition());
        }
    }
}
