using System;

namespace Tests.Diagnostics
{
    class Program
    {
        private int field = 1_000;

        public void CompliantCases()
        {
            var thousand = 1000;

            // decimal notation
            var balance = 2_435_951.68;
            double multiGroupLengths = 1_234.56_78;
            var groupsBeforDot = 1_234.11111;

            // hexadecimal notation
            var num = 0x01_00;

            // binary notation
            var num2 = 0b1_0000_0000;
            var num3 = 0b1_00_00_00_00;

            balance += 227_652;

            Console.WriteLine(1_234);

            // number types suffixes
            ulong ulong1 = 1_000_000UL;
            ulong ulong2 = 1_000_000ul;
            ulong ulong3 = 1_000_000uL;
            long long1 = 10_000L;
            long long2 = 10_000l;
            double double1 = 123.456D;
            double double2 = 123.456d;
            float float1 = 100.50F;
            float float2 = 100.50f;
            uint uint1 = 1_000U;
            uint uint2 = 1_000u;
            decimal decimal1 = 1_000.123M;
            decimal decimal2 = 1_000.123m;
        }

        public void NonCompliantCases()
        {
            int million = 1_000_00_000;  // Noncompliant {{Review this number; its irregular pattern indicates an error.}}
//                        ^^^^^^^^^^^^

            var x = 1_234.56_78_123; // Noncompliant

            var y = 1_234_5; // Noncompliant

            var num2 = 0b1_00_00_0000; // Noncompliant

            var num = 0x01_00_000; // Noncompliant
        }
    }
}
