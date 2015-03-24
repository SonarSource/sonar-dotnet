using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
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
