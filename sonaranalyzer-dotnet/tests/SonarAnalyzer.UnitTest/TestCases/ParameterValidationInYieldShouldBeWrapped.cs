using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public static class InvalidCases
    {
        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the iterator.}}
//                                         ^^^^^^^^^
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
//                                          ^^^^^^^^^^^^^^^^^^^^^ Secondary
            if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); }
//                                             ^^^^^^^^^^^^^^^^^^^^^ Secondary

            foreach (var element in source)
            {
                if (!predicate(element)) { break; }
                yield return element;
            }
        }

        public static IEnumerable<int> GetSomething(string value) // Noncompliant - this is an edge case that might be worth handling later on
        {
            yield return 42;

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value)); // Secondary
            }
        }

        public static IEnumerable<TSource> TakeWhile2<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) // Noncompliant - Not sure what should be the expected result (i.e. do local functions suffer from this behavior?)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); } // Secondary
            if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); } // Secondary

            return TakeWhileIterator<TSource>(source, predicate);

            IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> s, Func<TSource, bool> p)
            {
                foreach (var element in s)
                {
                    if (!p(element)) { break; }
                    yield return element;
                }
            }
        }
    }

    public static class ValidCases
    {
        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
            if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); }
            return TakeWhileIterator<TSource>(source, predicate);
        }

        private static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource element in source)
            {
                if (!predicate(element))
                    break;
                yield return element;
            }
        }

        public static IEnumerable<int> GetSomething() // Compliant - no args check
        {
            yield return 42;
        }

        public static IEnumerable<string> Get(int val)
        {
            if (val == 0)
            {
                yield break;
            }

            yield return "test";
        }
    }
}
