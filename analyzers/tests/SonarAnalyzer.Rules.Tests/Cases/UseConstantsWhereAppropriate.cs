using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        static readonly bool x1 = false;   // Noncompliant {{Replace this 'static readonly' declaration with 'const'.}}
        static readonly byte x2 = 1;       // Noncompliant
        static readonly sbyte x3 = 1;      // Noncompliant
        static readonly char x4 = 'a';     // Noncompliant
        static readonly decimal x5 = 1;    // Noncompliant
        static readonly double x6 = 1;     // Noncompliant
        static readonly float x7 = 1;      // Noncompliant
        static readonly int x8 = 1;        // Noncompliant
        static readonly uint x9 = 1;       // Noncompliant
        static readonly long x10 = 1;      // Noncompliant
        static readonly ulong x11 = 1;     // Noncompliant
        static readonly short x12 = 1;     // Noncompliant
        static readonly ushort x13 = 1;    // Noncompliant
        static readonly string x14 = "";   // Noncompliant

        static readonly int maxVal = int.MaxValue; // Noncompliant

        public static readonly int publicMaxVal = int.MaxValue; // Compliant- public
        protected static readonly int protectedMaxVal = int.MaxValue; // Compliant - protected

        static readonly object x15 = null; // Compliant

        static readonly bool y1;     // Compliant
        static readonly byte y2;     // Compliant
        static readonly sbyte y3;    // Compliant
        static readonly char y4;     // Compliant
        static readonly decimal y5;  // Compliant
        static readonly double y6;   // Compliant
        static readonly float y7;    // Compliant
        static readonly int y8;      // Compliant
        static readonly uint y9;     // Compliant
        static readonly long y10;    // Compliant
        static readonly ulong y11;   // Compliant
        static readonly short y12;   // Compliant
        static readonly ushort y13;  // Compliant
        static readonly string y14;  // Compliant

        static readonly string empty = string.Empty; // Compliant
        static readonly int myValue = GetMyValue(); // Compliant
        static readonly string[] values = { "a", "b", "c" }; // Compliant

        static int GetMyValue()
        {
            return 0;
        }
    }
}
