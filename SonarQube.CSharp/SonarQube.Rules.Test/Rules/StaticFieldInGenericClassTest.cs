using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class StaticFieldInGenericClassTest
    {
        [TestMethod]
        public void StaticFieldInGenericClass()
        {
            Verifier.Verify(@"TestCases\StaticFieldInGenericClass.cs", new StaticFieldInGenericClass());
        }
    }
}
