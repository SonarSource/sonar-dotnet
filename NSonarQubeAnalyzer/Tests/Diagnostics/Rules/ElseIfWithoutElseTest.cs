using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class ElseIfWithoutElseTest
    {
        [TestMethod]
        public void ElseIfWithoutElse()
        {
            Verifier.Verify(@"TestCases\ElseIfWithoutElse.cs", new ElseIfWithoutElse());
        }
    }
}
