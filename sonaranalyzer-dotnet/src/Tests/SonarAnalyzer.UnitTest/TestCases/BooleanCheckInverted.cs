using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class BooleanCheckInverted
    {
        public void Test()
        {
            var a = 2;
            if (!((a == 2))) // Noncompliant {{Use the opposite operator ('!=') instead.}}
//              ^^^^^^^^^^^
            {

            }
            bool b = !(a < 10);  // Noncompliant
//                   ^^^^^^^^^
            b = !(a <= 10);  // Noncompliant
            b = !(a > 10);  // Noncompliant
            b = !(a >= 10);  // Noncompliant
            b = !(a == 10);  // Noncompliant
            b = !(a != 10);  // Noncompliant


            if (a != 2)
            {
            }
            b = (a >= 10);

            var c = true && !(new int[0].Length == 0); // Noncompliant

            int[] args = { };
            var a = !!(args.Length == 0); // Noncompliant

            SomeFunc(!(a >= 10)); // Noncompliant
        }

        public static void SomeFunc(bool x) { }

        public static bool operator ==     (BooleanCheckInverted a, BooleanCheckInverted b)
        {
            return false;
        }

        public static bool operator !=(BooleanCheckInverted a, BooleanCheckInverted b)
        {
            return !(a==b); // Compliant
        }
    }
}
