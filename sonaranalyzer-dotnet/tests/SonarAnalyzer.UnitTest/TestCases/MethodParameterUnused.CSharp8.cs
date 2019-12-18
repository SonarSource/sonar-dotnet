namespace SonarAnalyzer.UnitTest.TestCases
{
    public class WithLocalFunctions
    {
        public void Method()
        {
            void M1(int a,
                    int b, // Noncompliant
                    int c,
                    int d) // Noncompliant
                    {
                        var result = a + c;
                    }

            static void M2(int a,
                           int b, // Noncompliant
                           int c,
                           int d) // Noncompliant
            {
                var result = a + c;
            }

            static int M3(int a,
                          int b, // Noncompliant
                          int c,
                          int d) // Noncompliant
                => a + c;
        }

    }
}
