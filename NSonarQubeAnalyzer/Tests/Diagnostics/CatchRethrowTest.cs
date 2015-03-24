using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
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
