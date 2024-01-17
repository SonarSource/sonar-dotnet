using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public static class InvalidCases
    {
        public static IEnumerable<string> YieldReturn(string something) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the iterator.}}
//                                        ^^^^^^^^^^^
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

        public static IEnumerable<string> IndirectUsageAsync(string something) // Noncompliant
        {
            var exception = new ArgumentNullException(nameof(something));
            if (something == null)
                throw exception; // Secondary
            yield return something;
        }

        public static IEnumerable<string> IndirectUsageWithMethodCallAsync(string something) // Noncompliant
        {
            if (something == null)
                throw GetArgumentExpression(nameof(something));
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
            yield return something;
        }

        private static ArgumentNullException GetArgumentExpression(string name)
        {
            return new ArgumentNullException(name);
        }

        // Documenting that this rule fires even if T:ArgumentException has no argument
        public static IEnumerable<int> ThrowWithoutArgument(int a) // Noncompliant
        {
            throw new ArgumentNullException(); // Secondary
            yield return 42;
        }

        // Documenting that this rule fires even if T:ArgumentException has an ad-hoc argument
        public static IEnumerable<int> ThrowWithAdHocArgument(int a) // Noncompliant
        {
            throw new ArgumentNullException("i am not a parameter name"); // Secondary
            yield return 42;
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

class NullCoalescing
{
    IEnumerable<int> Invalid(int? i)                // FN
    {
        _ = i ?? throw new ArgumentNullException(nameof(i));

        yield return 1;
    }

    IEnumerable<int> ValidWithSecondMethod(int? i)  // Compliant
    {
        _ = i ?? throw new ArgumentNullException(nameof(i));
        return SecondMethod();
    }

    IEnumerable<int> SecondMethod() { yield return 1; }

    IEnumerable<int> ValidWithLocalFunction(int? i)  // Compliant
    {
        _ = i ?? throw new ArgumentNullException(nameof(i));
        return LocalFunction();

        IEnumerable<int> LocalFunction() { yield return 1; }
    }
}
