namespace SonarAnalyzer.UnitTest.TestCases
{
    public interface IUtilities
    {
        public int MagicNum { get { return 42; } } // Compliant, inside interface

        private static string magicWord = "please";

        public string MagicWord // Compliant, inside interface
        {
            get { return magicWord; }
            set { magicWord = value; }
        }

        public int Sum(int a, int b) // Compliant, inside interface
        {
            return a + b;
        }
    }
}
