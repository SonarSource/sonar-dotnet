using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
{
    [TestClass]
    public class EmptyNestedBlockTest
    {
        [TestMethod]
        public void EmptyNestedBlock()
        {
            Verifier.Verify(@"TestCases\EmptyNestedBlock.cs", new EmptyNestedBlock());
        }
    }
}
