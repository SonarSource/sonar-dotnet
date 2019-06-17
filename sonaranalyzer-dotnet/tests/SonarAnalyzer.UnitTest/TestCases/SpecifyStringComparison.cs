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
        public void MyMethod1(string value1) { }
        public void MyMethod1(string value1, StringComparison comparisonType) { }

        public void MyMethod2(string value) { }

        [Obsolete]
        public void MyMethod2(string value, StringComparison comparisonType) { }

        public void MyMethod3(StringComparison comparisonType, string format, params string[] args) { }
        public void MyMethod3(string format, params string[] args) { }

        public void MyMethod4(string foo, params string[] args) { }
        public void MyMethod4(string foo, int bar, params string[] args) { }

        public void MyMethod5(StringComparison comparisonType, string format, params object[] args) { }
        public void MyMethod5(string format, params string[] args) { }

        public static void Contains<T>(T expected, IEnumerable<T> collection) { }
        public static void Contains<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer) { }
        public static void Contains<T>(IEnumerable<T> collection, Predicate<T> filter) { }

        public static void Contains2<T>(T expected, IEnumerable<T> collection) { }
        public static void Contains2<T>(T expected, IEnumerable<T> collection, StringComparison comparisonType) { }
        public static void Contains2<T>(IEnumerable<T> collection, Predicate<T> filter) { }

        public static void Contains3<T>(T expected, IEnumerable<T> collection) { }
        public static void Contains3<T, U>(T expected, IEnumerable<U> collection, StringComparison comparisonType) { }

        void InvalidCalls()
        {
            string.Compare("a", "b"); // Noncompliant {{Change this call to 'string.Compare' to an overload that accepts a 'StringComparison' as a parameter.}}

            string.Equals("a", "b"); // Noncompliant

            MyMethod("a"); // Noncompliant

            MyMethod1("a"); // Noncompliant

            MyMethod3("a"); // Noncompliant
            MyMethod3("a", "b"); // Noncompliant
            MyMethod3("a", "b", "c"); // Noncompliant

            MyMethod5("a", "b", "c"); // Noncompliant

            "".StartsWith(""); // Noncompliant

            Contains2("", new[] { "" }); // Noncompliant
            Contains3("", new[] { "" }); // FN - too complex to correctly resolve type arguments when they are different between invoked and overloaded methods
        }

        void ValidCalls()
        {
            string.Compare("a", "b", StringComparison.OrdinalIgnoreCase);
            string.Equals("a", "b", StringComparison.InvariantCulture);
            MyMethod("a", StringComparison.CurrentCulture);
            "a".IndexOf('#');

            "".StartsWith("", StringComparison.CurrentCulture); // Compliant
            "".StartsWith("", true, CultureInfo.CurrentCulture); // Compliant - CultureInfo implies string formatting

            MyMethod1("", ""); // Compliant
            MyMethod1("", StringComparison.CurrentCulture); // Compliant
            MyMethod2(""); // Compliant

            MyMethod3(StringComparison.OrdinalIgnoreCase, ""); // Compliant
            MyMethod3(StringComparison.OrdinalIgnoreCase, "", ""); // Compliant

            MyMethod4(""); // Compliant
            MyMethod4("", ""); // Compliant
            MyMethod4("", 1); // Compliant
            MyMethod4("", "", ""); // Compliant
            MyMethod4("", 1, ""); // Compliant

            MyMethod5(StringComparison.OrdinalIgnoreCase, "a", "b", "c"); // Compliant

            Contains("", new[] { "" }); // Compliant
            Contains("", new[] { "" }, StringComparer.OrdinalIgnoreCase); // Compliant

            Contains2("", new[] { "" }, StringComparison.OrdinalIgnoreCase); // Compliant
        }
    }
}
