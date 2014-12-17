namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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