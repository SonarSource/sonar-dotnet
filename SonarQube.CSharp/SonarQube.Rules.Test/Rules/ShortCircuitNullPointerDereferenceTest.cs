using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
