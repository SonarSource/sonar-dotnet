using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class ElseIfWithoutElseTest
    {
        [TestMethod]
        public void ElseIfWithoutElse()
        {
            Verifier.Verify(@"TestCases\ElseIfWithoutElse.cs", new ElseIfWithoutElse());
        }
    }
}
