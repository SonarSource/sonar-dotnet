using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class ShortCircuitNullPointerDereferenceTest
    {
        [TestMethod]
        public void ShortCircuitNullPointerDereference()
        {
            Verifier.Verify(@"TestCases\ShortCircuitNullPointerDereference.cs", new ShortCircuitNullPointerDereference());
        }
    }
}
