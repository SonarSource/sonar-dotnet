using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
