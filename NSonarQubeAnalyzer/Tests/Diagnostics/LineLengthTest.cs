using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class LineLengthTest
    {
        [TestMethod]
        public void LineLength()
        {
            var diagnostic = new LineLength();
            diagnostic.Maximum = 47;
            Verifier.Verify(@"TestCases\LineLength.cs", diagnostic);
        }
    }
}
