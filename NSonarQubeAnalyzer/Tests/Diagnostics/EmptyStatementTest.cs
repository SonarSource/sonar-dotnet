using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class EmptyStatementTest
    {
        [TestMethod]
        public void EmptyStatement()
        {
            Verifier.Verify(@"TestCases\EmptyStatement.cs", new EmptyStatement());
        }
    }
}
