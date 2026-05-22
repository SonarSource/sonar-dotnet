using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Net;

namespace Tests.Diagnostics
{
    class Program
    {
        void TestCases()
        {
            int result;
            int.TryParse("123", out result); // Compliant - Controversial: see examples below
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8233
    class Repro_8233
    {
        public void IgnoreCulture()
        {
            // Need more example of type implementating IParseable that ignore culture
            _ = Guid.Parse("");                 // Compliant
            _ = Guid.TryParse("", out _);       // Compliant

            // Controversial: There are edge cases like culture "fa-AF" where the 0x2212 (MINUS SIGN) is used instead of the usual 0x002D (HYPHEN-MINUS)
            _ = short.Parse("-1");              // Compliant
            _ = short.TryParse("-1", out _);    // Compliant
            _ = int.Parse("-1");                // Compliant
            _ = int.TryParse("-1", out _);      // Compliant
            _ = long.Parse("-1");               // Compliant
            _ = long.TryParse("-1", out _);     // Compliant
            _ = Int128.Parse("-1");             // Compliant
            _ = Int128.TryParse("-1", out _);   // Compliant

            // Floating point numbers should not be ignored as they are more culture-sensitive
            // e.g.: en-US -> 1,000.5 | de-DE -> 1.000,5
            _ = float.Parse("-1");              // Noncompliant
            _ = float.TryParse("-1", out _);    // Noncompliant
            _ = double.Parse("-1");             // Noncompliant
            _ = double.TryParse("-1", out _);   // Noncompliant

            // The following only have non-public overloads that take an IFormatProvider or CultureInfo
            // Where the overload does not use the IFormatProvider or CultureInfo
            _ = Boolean.Parse("");              // Compliant
            _ = Boolean.TryParse("", out _);    // Compliant
            _ = char.Parse("");                 // Compliant
            _ = char.TryParse("", out _);       // Compliant
            _ = IPAddress.Parse("");            // Compliant
            _ = IPAddress.TryParse("", out _);  // Compliant
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-230
    // C#13 introduced params collections (ReadOnlySpan<T>, List<T>, IEnumerable<T>, etc.) as an alternative to arrays.
    class Repro_NET230
    {
        static string UserMethodList(string format, params List<object> args) => format;
        static string UserMethodList(IFormatProvider provider, string format, params List<object> args) => format;

        static string UserMethodEnumerable(string format, params IEnumerable<object> args) => format;
        static string UserMethodEnumerable(IFormatProvider provider, string format, params IEnumerable<object> args) => format;

        static string UserMethodSpan(string format, params Span<object> args) => format;
        static string UserMethodSpan(IFormatProvider provider, string format, params Span<object> args) => format;

        static string UserMethodReadOnlySpan(string format, params ReadOnlySpan<object> args) => format;
        static string UserMethodReadOnlySpan(IFormatProvider provider, string format, params ReadOnlySpan<object> args) => format;

        static string UserMethodExtraParam(string format, string extra, params List<int> args) => format;
        static string UserMethodExtraParam(IFormatProvider provider, string format, params List<int> args) => format;

        static string UserMethodCustomCollection(string format, params MyCollection args) => format;
        static string UserMethodCustomCollection(IFormatProvider provider, string format, params MyCollection args) => format;

        void M()
        {
            string.Format("bla");                                                   // Noncompliant
            string.Format("%s %s", "foo", "bar", "quix", "hi", "bye");              // Noncompliant
            UserMethodList("hello");                                                // Noncompliant
            UserMethodList(CultureInfo.InvariantCulture, "hello");                  // Compliant
            UserMethodEnumerable("hello");                                          // Noncompliant
            UserMethodEnumerable(CultureInfo.InvariantCulture, "hello");            // Compliant
            UserMethodSpan("hello");                                                // Noncompliant
            UserMethodSpan(CultureInfo.InvariantCulture, "hello");                  // Compliant
            UserMethodReadOnlySpan("hello");                                        // Noncompliant
            UserMethodReadOnlySpan(CultureInfo.InvariantCulture, "hello");          // Compliant
            UserMethodExtraParam("hello", "extra");                                 // Compliant - 'string' is not compatible with List<int> element type, so no IFormatProvider overload matches
            UserMethodCustomCollection("hello");                                    // Noncompliant
            UserMethodCustomCollection(CultureInfo.InvariantCulture, "hello");      // Compliant
        }
    }

    class MyCollection : IEnumerable<object>
    {
        public void Add(object item) { }
        public IEnumerator<object> GetEnumerator() => throw new NotImplementedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
