using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class CatchRethrowTest
    {
        [TestMethod]
        public void CatchRethrow()
        {
            Verifier.Verify(@"TestCases\CatchRethrow.cs", new CatchRethrow());
        }
    }
}
