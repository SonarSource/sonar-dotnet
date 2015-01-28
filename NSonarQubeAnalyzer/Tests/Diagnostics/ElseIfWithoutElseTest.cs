namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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