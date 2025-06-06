using System;

namespace Tests.Diagnostics
{
    using static System.GC;

    class Program
    {
        void Foo()
        {
            GC.Collect(); // Noncompliant {{Refactor the code to remove this use of 'GC.Collect'.}}
//             ^^^^^^^
            GC.Collect(2, GCCollectionMode.Optimized); // Noncompliant

            Collect(); // Noncompliant

            // Repro for: https://github.com/SonarSource/sonar-dotnet/issues/9687
            GC.GetTotalMemory(true);  // Noncompliant {{Refactor the code to remove this use of 'GC.GetTotalMemory'.}}
            GC.GetTotalMemory(false); // Compliant
            GC.GetTotalMemory(forceFullCollection: true); // Noncompliant
            GC.GetTotalMemory(forceFullCollection: false); // Compliant
        }
    }
}
