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

        public static IEnumerable<int> GetSomething(string value) // FN - this is an edge case that might be worth handling later on
        {
            yield return 42;

            if (value == null)
            {
                ArgumentNullException.ThrowIfNull(value); // FN sec
            }
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

        public static IEnumerable<string> WithLocalFunction(string something) // Compliant - usage of local function
        {
            ArgumentNullException.ThrowIfNull(something);

            return Iterator(something);

            IEnumerable<string> Iterator(string s)
            {
                yield return s;
            }
        }

        public static IEnumerable<int> WithFunc(string foo)
        {
            Func<string, string> func =
                f =>
                {
                    ArgumentNullException.ThrowIfNull(f);

                    return f + f;
                };

            yield return foo.Length;
        }
    }
}
