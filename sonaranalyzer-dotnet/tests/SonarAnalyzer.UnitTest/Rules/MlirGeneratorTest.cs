extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MlirGeneratorTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void MlirGenerator()
        {
            Verifier.VerifyAnalyzer(@"TestCases\MlirGenerator.cs", new MlirGenerator());
        }
    }
}
