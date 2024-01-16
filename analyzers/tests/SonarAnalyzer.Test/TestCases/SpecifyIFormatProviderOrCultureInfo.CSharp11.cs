using System;
using System.Globalization;
using System.Resources;

namespace Tests.Diagnostics
{
    class Program
    {
        void TestCases()
        {
            int result;
            int.TryParse("123", out result); // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8233
    class Repro_8233
    {
        public void IgnoreCulture()
        {
            // Need more example of type implementating IParseable that ignore culture
            _ = Guid.Parse(""); // Noncompliant - FP
            // This should be noncompliant see: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Boolean.cs#L410
            // Need some investigation on why it does not raise.
            _ = Boolean.Parse(""); // Compliant - FN (as of today's implementation)
            _ = int.Parse("-1"); // Noncompliant - FP Controversial: There are edge cases like culture "fa-AF" where the 0x2212 (MINUS SIGN) is used instead of the usual 0x002D (HYPHEN-MINUS)
        }
    }
}
