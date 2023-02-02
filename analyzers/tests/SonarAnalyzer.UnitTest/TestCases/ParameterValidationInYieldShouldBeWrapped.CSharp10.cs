using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public static class InvalidCases
    {
        public static IEnumerable<string> YieldReturn(string something) // Noncompliant
        {
            ArgumentNullException.ThrowIfNull(something);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

            yield return something;
        }

        public static IEnumerable<int> YieldBreak(string something) // Noncompliant
        {
            ArgumentNullException.ThrowIfNull(something);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

            yield break;
        }

        public static IEnumerable<int> ThrowExpression(string something) // FN #6369
        {
            _ = something ?? throw new ArgumentNullException();

            yield break;
        }

        // For details, check https://github.com/SonarSource/sonar-dotnet/pull/6624.
        public static async IAsyncEnumerable<int> AsyncThenYield(object arg) // Noncompliant
        {
            if (arg is null)
            {
                throw new ArgumentException(nameof(arg));
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
            }

            var res = await Task.Run(() => 42);
            yield return res;
        }

        // For details, check https://github.com/SonarSource/sonar-dotnet/pull/6624.
        public static async IAsyncEnumerable<int> NestedAsyncThenYield(object arg) // Noncompliant
        {
            if (arg is null)
            {
                throw new ArgumentException(nameof(arg));
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
            }

            var res = (await Task.Run(() => 42)).GetHashCode();
            yield return res;
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
