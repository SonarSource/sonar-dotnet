using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
