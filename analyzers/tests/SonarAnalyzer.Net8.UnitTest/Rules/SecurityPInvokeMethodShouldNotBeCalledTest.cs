using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Net8.UnitTest.Rules;

[TestClass]
public class SecurityPInvokeMethodShouldNotBeCalledTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.SecurityPInvokeMethodShouldNotBeCalled>().WithOptions(ParseOptionsHelper.FromCSharp12);

    [TestMethod]
    public void SecurityPInvokeMethodShouldNotBeCalled() =>
        builderCS.AddPaths("SecurityPInvokeMethodShouldNotBeCalled.cs").Verify();
}
