namespace SonarAnalyzer.UnitTest.TestCases.RoslynCFGComparer
{
    public class LocalFunctions
    {
        public void StaticLocalFunctionCall()
        {
            var result = Compute(1, 2);

            static int Compute(int a, int b) => a + b;
        }

        public void StaticLocalNestedFunctions()
        {
            var result = Compute(1, 2);

            static int Compute(int a, int b)
            {
                return ComputeNested(a, b);

                static int ComputeNested(int c, int d)
                {
                    return c + d;
                }
            }
        }

        public void LocalFunctionNoCapture()
        {
            var result = Compute(1, 3);

            int Compute(int a, int b) => a + b;
        }

        public void LocalFunctionCapturesLocalVariables()
        {
            int a = 1, b = 2;

            var result = a + b;

            int Compute() => a + b;
        }
    }
}
