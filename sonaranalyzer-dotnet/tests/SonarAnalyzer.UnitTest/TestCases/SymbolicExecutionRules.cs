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

                if (resource == null) // Noncompliant - FP resource can be null
                { throw new Exception(); }
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
            }

            private static IDisposable GetNullResource() => null;
        }
    }
}
