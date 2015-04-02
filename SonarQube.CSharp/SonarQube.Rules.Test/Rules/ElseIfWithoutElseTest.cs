using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
