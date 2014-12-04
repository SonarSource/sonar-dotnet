using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

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
