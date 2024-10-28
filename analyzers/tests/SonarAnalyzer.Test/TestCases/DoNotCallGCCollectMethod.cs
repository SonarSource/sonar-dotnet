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
            GC.GetTotalMemory(true);  // FN, when forceFullCollection is true the method calls GC.Collect() see: https://github.com/dotnet/runtime/blob/fe3e5437c0a4c51ae1f9c5e9ebe142dc41c8feba/src/coreclr/System.Private.CoreLib/src/System/GC.CoreCLR.cs#L391
            GC.GetTotalMemory(false); // Compliant
        }
    }
}
