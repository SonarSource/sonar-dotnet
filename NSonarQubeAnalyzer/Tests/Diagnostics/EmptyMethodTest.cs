namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

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