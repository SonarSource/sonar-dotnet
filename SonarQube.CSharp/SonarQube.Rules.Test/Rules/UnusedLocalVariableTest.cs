using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class UnusedLocalVariableTest
    {
        [TestMethod]
        public void UnusedLocalVariable()
        {
            Verifier.Verify(@"TestCases\UnusedLocalVariable.cs", new UnusedLocalVariable());
        }

        
    }
}
