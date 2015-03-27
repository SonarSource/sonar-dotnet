using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class ExpressionComplexityTest
    {
        [TestMethod]
        public void ExpressionComplexity()
        {
            var diagnostic = new ExpressionComplexity {Maximum = 3};
            Verifier.Verify(@"TestCases\ExpressionComplexity.cs", diagnostic);
        }
    }
}
