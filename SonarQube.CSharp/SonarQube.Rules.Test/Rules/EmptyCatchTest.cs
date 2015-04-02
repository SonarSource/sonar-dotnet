using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
