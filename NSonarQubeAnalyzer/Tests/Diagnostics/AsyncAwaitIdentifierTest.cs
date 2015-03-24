using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class AsyncAwaitIdentifierTest
    {
        [TestMethod]
        public void AsyncAwaitIdentifier()
        {
            Verifier.Verify(@"TestCases\AsyncAwaitIdentifier.cs", new AsyncAwaitIdentifier());
        }
    }
}
