using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
