using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public static class InvalidCases
    {
        public static IEnumerable<string> Foo(string something) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the iterator.}}
//                                        ^^^
        {
            if (something == null) { throw new ArgumentNullException(nameof(something)); }
//                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

            yield return something;
        }

        public static IEnumerable<int> GetSomething(string value) // Noncompliant - this is an edge case that might be worth handling later on
        {
            yield return 42;

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value)); // Secondary
            }
        }

        public static IEnumerable<int> YieldBreak(int a) // Noncompliant
        {
            if (a < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(a)); // Secondary
            }

            yield break;
        }
    }

    public static class ValidCases
    {
        public static IEnumerable<string> Foo(string something) // Compliant - split into 2 methods
        {
            if (something == null) { throw new ArgumentNullException(nameof(something)); }

            return FooIterator(something);
        }

        private static IEnumerable<string> FooIterator(string something)
        {
            yield return something;
        }

        public static IEnumerable<string> WithLocalFunction(string something) // Compliant - usage of local function
        {
            if (something == null) { throw new ArgumentNullException(nameof(something)); }

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
                    if (f == null)
                    {
                        throw new ArgumentNullException(nameof(f));
                    }

                    return f + f;
                };

            yield return foo.Length;
        }
    }
}
