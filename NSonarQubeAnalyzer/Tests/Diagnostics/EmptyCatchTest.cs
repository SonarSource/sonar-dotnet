using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class EmptyCatchTest
    {
        [TestMethod]
        public void EmptyCatch()
        {
            Verifier.Verify(@"TestCases\EmptyCatch.cs", new EmptyCatch());
        }
    }
}
