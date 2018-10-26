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
            bool ba = !!(args.Length == 0); // Noncompliant

            SomeFunc(!(a >= 10)); // Noncompliant
        }

        public void TestNullables()
        {
            int? a = 5;

            bool b = !(a < 5); // Compliant
            b = !(a <= 5); // Compliant
            b = !(a > 5); // Compliant
            b = !(a >= 5); // Compliant
            b = !(a == 5); // Noncompliant
            b = !(a != 5); // Noncompliant
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
