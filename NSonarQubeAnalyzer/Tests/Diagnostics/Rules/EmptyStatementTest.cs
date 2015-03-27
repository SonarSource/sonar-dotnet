using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
