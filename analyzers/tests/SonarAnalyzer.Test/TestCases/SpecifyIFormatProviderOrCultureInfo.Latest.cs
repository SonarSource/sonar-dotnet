using System;
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
}
