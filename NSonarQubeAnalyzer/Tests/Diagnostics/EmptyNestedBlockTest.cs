using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

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
