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
        }
    }
}
