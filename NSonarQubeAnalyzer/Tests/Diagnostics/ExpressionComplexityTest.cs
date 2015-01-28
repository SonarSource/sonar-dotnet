namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

    [TestClass]
    public class ExpressionComplexityTest
    {
        [TestMethod]
        public void ExpressionComplexity()
        {
            var diagnostic = new ExpressionComplexity();
            diagnostic.Maximum = 3;
            Verifier.Verify(@"TestCases\ExpressionComplexity.cs", diagnostic);
        }
    }
}