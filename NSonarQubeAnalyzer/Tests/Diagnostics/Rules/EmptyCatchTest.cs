using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class EmptyCatchTest
    {
        [TestMethod]
        public void EmptyCatch()
        {
            Verifier.Verify(@"TestCases\EmptyCatch.cs", new EmptyCatch());
        }
    }
}
