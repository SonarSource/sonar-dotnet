using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
