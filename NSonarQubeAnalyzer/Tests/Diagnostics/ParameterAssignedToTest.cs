using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
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
