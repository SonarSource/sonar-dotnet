using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
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
