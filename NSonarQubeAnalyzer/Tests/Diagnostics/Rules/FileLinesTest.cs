using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
{
    [TestClass]
    public class FileLinesTest
    {
        [TestMethod]
        public void FileLines()
        {
            var diagnostic = new FileLines {Maximum = 12};
            Verifier.Verify(@"TestCases\FileLines12.cs", diagnostic);
            Verifier.Verify(@"TestCases\FileLines13.cs", diagnostic);
        }
    }
}
