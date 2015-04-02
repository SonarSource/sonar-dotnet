using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class MultilineBlocksWithoutBraceTest
    {
        [TestMethod]
        public void MultilineBlocksWithoutBrace()
        {
            Verifier.Verify(@"TestCases\MultilineBlocksWithoutBrace.cs", new MultilineBlocksWithoutBrace());
        }
    }
}
