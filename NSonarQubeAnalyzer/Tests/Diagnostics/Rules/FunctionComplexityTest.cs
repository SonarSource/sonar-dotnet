using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
