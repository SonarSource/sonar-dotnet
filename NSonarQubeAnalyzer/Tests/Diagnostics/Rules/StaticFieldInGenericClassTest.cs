using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
