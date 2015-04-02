using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
