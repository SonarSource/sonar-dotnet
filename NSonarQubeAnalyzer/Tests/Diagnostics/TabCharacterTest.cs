namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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