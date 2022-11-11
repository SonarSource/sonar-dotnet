using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public static class InvalidCases
    {
        public static IEnumerable<string> Foo(string something) // FN
        {
            ArgumentNullException.ThrowIfNull(something); // FN sec

            yield return something;
        }

        public static IEnumerable<int> YieldBreak(int a) // FN
        {
            if (a < 0)
            {
                ArgumentNullException.ThrowIfNull(a); // FN sec
            }

            yield break;
        }
    }

    public static class ValidCases
    {
        public static IEnumerable<string> Foo(string something) // Compliant - split into 2 methods
        {
            ArgumentNullException.ThrowIfNull(something);

            return FooIterator(something);
        }

        private static IEnumerable<string> FooIterator(string something)
        {
            yield return something;
        }
    }
}
