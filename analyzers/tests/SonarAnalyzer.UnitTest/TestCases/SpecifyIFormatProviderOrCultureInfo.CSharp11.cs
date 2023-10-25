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
        }
    }
}
