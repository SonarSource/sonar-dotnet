using System;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class FunctionComplexityTest
    {
        [TestMethod]
        public void FunctionComplexity()
        {
            var diagnostic = new FunctionComplexity();
            diagnostic.Maximum = 3;
            Verifier.Verify(@"TestCases\FunctionComplexity.cs", diagnostic);
        }
    }
}
