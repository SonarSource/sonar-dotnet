using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
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
