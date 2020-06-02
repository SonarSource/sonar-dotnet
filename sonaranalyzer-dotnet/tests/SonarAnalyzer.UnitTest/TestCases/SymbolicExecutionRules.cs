using System;

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class SymbolicExecutionRules
    {
        internal static class Program
        {
            public static void Main()
            {
                using var resource = GetNullResource();

                if (resource == null)
                { throw new Exception(); }
            }

            private static IDisposable GetNullResource() => null;
        }
    }
}
