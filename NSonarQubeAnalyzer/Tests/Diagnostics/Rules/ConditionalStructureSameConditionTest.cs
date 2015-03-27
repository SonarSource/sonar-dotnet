using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
