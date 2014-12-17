namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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