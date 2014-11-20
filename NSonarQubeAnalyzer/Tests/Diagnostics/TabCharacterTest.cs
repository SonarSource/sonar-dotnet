using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests.Diagnostics
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
