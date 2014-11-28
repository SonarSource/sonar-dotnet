using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class TooManyParametersTest
    {
        [TestMethod]
        public void TooManyParameters()
        {
            var diagnostic = new TooManyParameters();
            diagnostic.Maximum = 3;
            Verifier.Verify(@"TestCases\TooManyParameters.cs", diagnostic);
        }
    }
}
