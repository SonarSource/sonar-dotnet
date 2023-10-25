using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Net8.UnitTest.Rules;

[TestClass]
public class PublicMethodWithMultidimensionalArrayTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.PublicMethodWithMultidimensionalArray>().WithOptions(ParseOptionsHelper.FromCSharp12);

    [TestMethod]
    public void PublicMethodWithMultidimensionalArray() =>
        builderCS.AddPaths("PublicMethodWithMultidimensionalArray.cs").Verify();
}
