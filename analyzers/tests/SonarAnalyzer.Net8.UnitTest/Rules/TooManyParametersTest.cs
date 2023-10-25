using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Net8.UnitTest.Rules;

[TestClass]
public class TooManyParametersTest
{
    private readonly VerifierBuilder builderCSMax3 = new VerifierBuilder()
                                                     .AddAnalyzer(() => new CS.TooManyParameters { Maximum = 3 })
                                                     .WithOptions(ParseOptionsHelper.FromCSharp12);

    [TestMethod]
    public void TooManyParameters_CS_CustomValues() =>
        builderCSMax3.AddPaths("TooManyParameters_CustomValues.cs").Verify();
}
