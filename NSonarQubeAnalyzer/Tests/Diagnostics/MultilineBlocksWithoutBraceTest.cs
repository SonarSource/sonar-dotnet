using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
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
