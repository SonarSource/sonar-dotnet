using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
