using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class EmptyMethodTest
    {
        [TestMethod]
        public void EmptyMethod()
        {
            Verifier.Verify(@"TestCases\EmptyMethod.cs", new EmptyMethod());
        }
    }
}
