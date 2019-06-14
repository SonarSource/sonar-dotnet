using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tests.Diagnostics
{
    class Program
    {
        public void MyMethod(string value) { }
        public void MyMethod(string value, StringComparison comparisonType) { }

        public void MyMethod1(string value1, string value2) { }
        public void MyMethod1(string value1, StringComparison comparisonType) { }

        public void MyMethod2(string value) { }

        [Obsolete]
        public void MyMethod2(string value, StringComparison comparisonType) { }

        public static void Contains<T>(T expected, IEnumerable<T> collection) { }

        public static void Contains<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer) { }

        public static void Contains<T>(IEnumerable<T> collection, Predicate<T> filter) { }

        void InvalidCalls()
        {
            string.Compare("a", "b"); // Noncompliant {{Change this call to 'string.Compare' to an overload that accepts a 'StringComparison' as a parameter.}}

            string.Equals("a", "b"); // Noncompliant

            MyMethod("a"); // Noncompliant

            "".StartsWith(""); // Noncompliant
        }

        void ValidCalls()
        {
            string.Compare("a", "b", StringComparison.OrdinalIgnoreCase);
            string.Equals("a", "b", StringComparison.InvariantCulture);
            MyMethod("a", StringComparison.CurrentCulture);
            "a".IndexOf('#');

            "".StartsWith("", StringComparison.CurrentCulture); // Compliant
            "".StartsWith("", true, CultureInfo.CurrentCulture); // Compliant - CultureInfo implies string formatting

            MyMethod1("", "");
            MyMethod2("");

            Contains("", new[] { "" }, StringComparer.OrdinalIgnoreCase); // Compliant
        }
    }
}
