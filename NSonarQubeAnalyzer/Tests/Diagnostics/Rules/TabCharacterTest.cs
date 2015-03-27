using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class TabCharacterTest
    {
        [TestMethod]
        public void TabCharacter()
        {
            Verifier.Verify(@"TestCases\TabCharacter.cs", new TabCharacter());
        }
    }
}
