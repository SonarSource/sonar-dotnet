using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class TooManyParameters : If
    {
        public static void F1(int p1, int p2, int p3) { }

        public static void F2(int p1, int p2, int p3, int p4) { } // Compliant, interface implementation

        public static void F1(int p1, int p2, int p3, int p4) { } // Noncompliant {{Method has 4 parameters, which is greater than the 3 authorized.}}
    }

    public interface If
    {
        static abstract void F1(int p1, int p2, int p3);
        static abstract void F2(int p1, int p2, int p3, int p4); // Noncompliant
    }
}
