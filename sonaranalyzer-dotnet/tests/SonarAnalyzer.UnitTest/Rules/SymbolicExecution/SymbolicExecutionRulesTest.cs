using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution
{
    [TestClass]
    public class SymbolicExecutionRulesTest
    {
        // This test is meant to run all the symbolic execution rules together and verify different scenarios.
        [TestMethod]
        [TestCategory("Rule")]
        public void VerifySymbolicExecutionRules() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecutionRules.cs",
                new SymbolicExecutionRunner(),
#if NETFRAMEWORK
                options: ParseOptionsHelper.FromCSharp8,
                additionalReferences: NuGetMetadataReference.NETStandardV2_1_0);
#else
                options: ParseOptionsHelper.FromCSharp8);
#endif
    }
}
