using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
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
