using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class ClassNameTest
    {
        [TestMethod]
        public void ClassName()
        {
            var diagnostic = new ClassName {Convention = "^(?:[A-HJ-Z][a-zA-Z0-9]+|I[a-z0-9][a-zA-Z0-9]*)$"};
            Verifier.Verify(@"TestCases\ClassName.cs", diagnostic);
        }
    }
}
