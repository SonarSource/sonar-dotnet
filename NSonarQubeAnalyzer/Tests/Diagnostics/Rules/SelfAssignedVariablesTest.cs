using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class SelfAssignedVariablesTest
    {
        [TestMethod]
        public void SelfAssignedVariables()
        {
            Verifier.Verify(@"TestCases\SelfAssignedVariables.cs", new SelfAssignedVariables());
        }
    }
}
