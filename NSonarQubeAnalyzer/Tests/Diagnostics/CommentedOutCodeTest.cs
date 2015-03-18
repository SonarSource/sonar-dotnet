using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
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
