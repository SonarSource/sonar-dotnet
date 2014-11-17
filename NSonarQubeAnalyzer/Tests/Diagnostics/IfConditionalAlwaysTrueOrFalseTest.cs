using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class IfConditionalAlwaysTrueOrFalseTest
    {
        [TestMethod]
        public void IfConditionalAlwaysTrueOrFalse()
        {
            Verifier.Verify(@"TestCases\IfConditionalAlwaysTrueOrFalse.cs", new IfConditionalAlwaysTrueOrFalse());
        }
    }
}
