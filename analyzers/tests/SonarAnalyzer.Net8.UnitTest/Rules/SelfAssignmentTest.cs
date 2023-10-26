using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.Net8.UnitTest.Rules;

[TestClass]
public class SelfAssignmentTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<SonarAnalyzer.Rules.CSharp.SelfAssignment>().WithConcurrentAnalysis(false);

    [TestMethod]
    public void SelfAssignment() =>
        builderCS.AddPaths("SelfAssignment.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();
}
