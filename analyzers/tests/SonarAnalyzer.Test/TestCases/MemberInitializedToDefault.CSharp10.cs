using System;

namespace Tests.Diagnostics
{
    public struct BarStruct
    {
        // Repro for issue: https://github.com/SonarSource/sonar-dotnet/issues/6461
        public int someField = 0; // Noncompliant FP for versions < C# 11 Fields that are not initialized in all constructors must be set explicitly.
        public BarStruct(bool someDummy) // Ctor with a parameter that does not initialize all fields
        { }
    }
}

