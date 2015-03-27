using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
