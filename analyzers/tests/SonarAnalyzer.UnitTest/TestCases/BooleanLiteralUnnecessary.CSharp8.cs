using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    // Repro for: https://github.com/SonarSource/sonar-dotnet/issues/5013
    public class CodeFixProviderRepro
    {
        public static bool Foo(string? x, bool y) => x == null ? false : y; // Noncompliant
    }
}
