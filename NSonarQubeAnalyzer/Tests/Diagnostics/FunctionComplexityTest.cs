using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class FunctionComplexityTest
    {
        [TestMethod]
        public void FunctionComplexity()
        {
            var diagnostic = new FunctionComplexity {Maximum = 3};
            Verifier.Verify(@"TestCases\FunctionComplexity.cs", diagnostic);
        }
    }
}
