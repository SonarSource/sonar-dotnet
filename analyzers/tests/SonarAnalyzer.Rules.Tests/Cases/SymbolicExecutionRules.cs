using System;

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class SymbolicExecutionRules
    {
        internal static class Program
        {
            public static void Main()
            {
                using var resource1 = GetNullResource();

                if (resource1 == null) // Compliant - "resource1" can be null
                {
                    throw new Exception();
                }

                var resource2 = GetNullResource();
                if (resource2 == null) // Compliant - "resource2" can be null
                {
                    throw new Exception();
                }

                using (var resource3 = GetNullResource())
                {
                    if (resource3 == null) // Compliant - "resource3" can be null
                    {
                        throw new Exception();
                    }
                }
            }

            private static IDisposable GetNullResource() => null;
        }
    }
}
