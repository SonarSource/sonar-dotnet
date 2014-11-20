using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class RightCurlyBraceStartsLineTest
    {
        [TestMethod]
        public void RightCurlyBraceStartsLine()
        {
            Verifier.Verify(@"TestCases\RightCurlyBraceStartsLine.cs", new RightCurlyBraceStartsLine());
        }
    }
}
