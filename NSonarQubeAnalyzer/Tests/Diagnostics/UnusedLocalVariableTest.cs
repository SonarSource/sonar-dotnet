using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class UnusedLocalVariableTest
    {
        [TestMethod]
        public void UnusedLocalVariable()
        {
            Verifier.Verify(@"TestCases\UnusedLocalVariable.cs", new UnusedLocalVariable());
        }
    }
}
