using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class EmptyMethodTest
    {
        [TestMethod]
        public void EmptyMethod()
        {
            Verifier.Verify(@"TestCases\EmptyMethod.cs", new EmptyMethod());
        }
    }
}
