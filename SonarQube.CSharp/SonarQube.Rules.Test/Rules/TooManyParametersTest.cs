using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class TooManyParametersTest
    {
        [TestMethod]
        public void TooManyParameters()
        {
            var diagnostic = new TooManyParameters {Maximum = 3};
            Verifier.Verify(@"TestCases\TooManyParameters.cs", diagnostic);
        }
    }
}
