using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
