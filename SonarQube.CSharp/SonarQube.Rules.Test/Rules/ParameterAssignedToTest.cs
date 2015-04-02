using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class ParameterAssignedToTest
    {
        [TestMethod]
        public void ParameterAssignedTo()
        {
            Verifier.Verify(@"TestCases\ParameterAssignedTo.cs", new ParameterAssignedTo());
        }
    }
}
