using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Net8.UnitTest.Rules;

[TestClass]
public class BypassingAccessibilityTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BypassingAccessibility>().WithOptions(ParseOptionsHelper.FromCSharp12);

    [TestMethod]
    public void BypassingAccessibility() =>
        builderCS.AddPaths("BypassingAccessibility.cs").Verify();
}
