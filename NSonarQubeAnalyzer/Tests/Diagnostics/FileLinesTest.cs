namespace Tests.Diagnostics
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;

    [TestClass]
    public class FileLinesTest
    {
        [TestMethod]
        public void FileLines()
        {
            var diagnostic = new FileLines();
            diagnostic.Maximum = 12;
            Verifier.Verify(@"TestCases\FileLines12.cs", diagnostic);
            Verifier.Verify(@"TestCases\FileLines13.cs", diagnostic);
        }

        [TestMethod]
        public void FileLinesWithDefault()
        {
            var diagnostic = new FileLines();
            Verifier.Verify(@"TestCases\FileLines12.cs", diagnostic);
            Verifier.Verify(@"TestCases\FileLines13.cs", diagnostic);
        }
    }
}