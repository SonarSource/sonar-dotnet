using System;

namespace Tests.Diagnostics
{
    internal record struct Noncompliant // FN
    {
        public static decimal A = 3.14m;
        private decimal E = 1m;

        public int PropertyA { get; }
        private int PropertyE { get; }

        public int GetA() => 1;
        private int GetE() => 1;
    }

    internal record struct NoncompliantPositionalRecord(string Property) // FN
    {
        public static decimal A = 3.14m;
    }
}
