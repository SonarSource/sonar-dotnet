using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
