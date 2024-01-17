using System;

namespace Tests.Diagnostics
{
    internal record struct Noncompliant // Noncompliant {{Types should not have members with visibility set higher than the type's visibility}}
    //                     ^^^^^^^^^^^^
    {
        public Noncompliant() { } // Secondary

        static public decimal A = 3.14m; // Secondary
        //     ^^^^^^
        private decimal E = 1m;

        public int PropertyA { get; } = 0; // Secondary
        private int PropertyE { get; } = 0;

        public int GetA() => 1; // Secondary
        private int GetE() => 1;
    }

    internal record struct NoncompliantPositionalRecord(string Property) // Noncompliant
    {
        public static decimal A = 3.14m; // Secondary
    }
}
