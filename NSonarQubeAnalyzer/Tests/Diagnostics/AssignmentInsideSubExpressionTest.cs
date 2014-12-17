namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

    [TestClass]
    public class AssignmentInsideSubExpressionTest
    {
        [TestMethod]
        public void AssignmentInsideSubExpression()
        {
            Verifier.Verify(@"TestCases\AssignmentInsideSubExpression.cs", new AssignmentInsideSubExpression());
        }
    }
}