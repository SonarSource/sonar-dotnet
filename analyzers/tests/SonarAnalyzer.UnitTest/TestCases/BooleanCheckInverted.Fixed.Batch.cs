using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class BooleanCheckInverted
    {
        public void Test()
        {
            var a = 2;
            if (a != 2) // Fixed
            {

            }
            bool b = a >= 10;  // Fixed
            b = a > 10;  // Fixed
            b = a <= 10;  // Fixed
            b = a < 10;  // Fixed
            b = a != 10;  // Fixed
            b = a == 10;  // Fixed


            if (a != 2)
            {
            }
            b = (a >= 10);

            var c = true && (new int[0].Length != 0); // Fixed

            int[] args = { };
            bool ba = !(args.Length != 0); // Fixed

            SomeFunc(a < 10); // Fixed
        }

        public void TestNullables()
        {
            int? a = 5;

            bool b = !(a < 5); // Compliant
            b = !(a <= 5); // Compliant
            b = !(a > 5); // Compliant
            b = !(a >= 5); // Compliant
            b = a != 5; // Fixed
            b = a == 5; // Fixed
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

        // Note that this is not the same as "collection?.Count <= 0" because when null is compared to a number, the result will always be false
        public static bool IsNullOrEmpty<T>(IList<T> collection) => !(collection?.Count > 0); // Compliant

        public static bool IsNullOrEmpty1(IList<int> collection) => !(collection?[0] > 0); // Compliant

        public static bool IsNullOrEmpty2<T>(IList<T> collection) => !(0 < collection?.Count); // Compliant

        public static bool IsNullOrEmpty3<T>(IList<T> collection) => !((0) < ((collection?.Count))); // Compliant

        public static bool IsNullOrEmpty4<T>(IList<T> collection) => collection?.Count != 0; // Fixed
    }
}
