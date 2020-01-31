namespace SonarCheck
{
    public partial class Partial
    {
        private int GetValue1()
        {
            return 1;
        }

        private int GetValue2() // Noncompliant
        {
            return 1;
        }
    }
}
