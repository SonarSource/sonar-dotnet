using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Linq.Expressions;

namespace Tests.Diagnostics
{
    public class StringOperationWithoutCulture
    {
        void Test()
        {
            var s = "";
            s = s.ToLower(); // Noncompliant
//                ^^^^^^^
            s = s.ToUpper(); // Noncompliant {{Define the locale to be used in this string operation.}}

            s = s.ToUpperInvariant();
            s = s.ToUpper(CultureInfo.InvariantCulture);
            var b = s.StartsWith("", StringComparison.CurrentCulture);
            b = s.StartsWith(""); // Compliant, although culture specific
            b = s.EndsWith(""); // Compliant, although culture specific
            b = s.StartsWith("", true, CultureInfo.InvariantCulture);
            b = s.Equals(""); // Compliant, ordinal compare
            b = s.Equals(new object());
            b = s.Equals("", StringComparison.CurrentCulture);
            var i = string.Compare("", "", true); // Noncompliant
            i = string.Compare("", 1, "", 2, 3, true); // Noncompliant
            i = string.Compare("", 1, "", 2, 3, true, CultureInfo.InstalledUICulture);
            i = string.Compare("", "", StringComparison.CurrentCulture);

            s = 1.8.ToString(); //Noncompliant
            s = 1.8m.ToString(); //Noncompliant
            s = 1.8f.ToString("d");
            s = 1.8.ToString(CultureInfo.InstalledUICulture);

            i = "".CompareTo(""); // Noncompliant {{Use 'CompareOrdinal' or 'Compare' with the locale specified instead of 'CompareTo'.}}
            object o = "";
            i = "".CompareTo(o); // Noncompliant

            i = "".IndexOf(""); // Noncompliant
            i = "".IndexOf('a');
            i = "".LastIndexOf(""); // Noncompliant
            i = "".LastIndexOf("", StringComparison.CurrentCulture);
        }

        void TestExpressions()
        {
            // Make sure we don't report it for Expressions
            Expression<Func<string, bool>> e = s => s.ToUpper() == "TOTO";
            Func<string, bool> f = s => s.ToUpper() == "TOTO";                              // Noncompliant
            var primes = new List<string> { "two", "three", "five", "seven" };
            var withT = primes.Where(s => s.ToUpper().StartsWith("T"));                     // Noncompliant
            var withTQuery = primes.AsQueryable().Where(s => s.ToUpper().StartsWith("T"));
            var withoutT = from s in primes where !s.ToUpper().StartsWith("T") select s;    // Noncompliant
            var withoutTQuery = from s in primes.AsQueryable() where !s.ToUpper().StartsWith("T") select s;
        }

        void TestDateTimeTypes()
        {
            var dateTime = new DateTime().ToString(); // Noncompliant

            // Repro for https://github.com/SonarSource/sonar-dotnet/issues/6439
            var dateTimeOffset = new DateTimeOffset().ToString(); // FN
        }
    }
}
