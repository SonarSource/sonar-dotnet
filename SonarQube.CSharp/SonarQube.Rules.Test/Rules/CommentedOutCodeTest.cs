using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class CommentedOutCodeTest
    {
        [TestMethod]
        public void CommentedOutCode()
        {
            Verifier.Verify(@"TestCases\CommentedOutCode.cs", new CommentedOutCode());
        }
    }
}
