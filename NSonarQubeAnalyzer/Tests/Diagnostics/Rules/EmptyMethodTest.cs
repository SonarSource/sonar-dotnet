using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class EmptyMethodTest
    {
        [TestMethod]
        public void EmptyMethod()
        {
            Verifier.Verify(@"TestCases\EmptyMethod.cs", new EmptyMethod());
        }
    }
}
