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
            var x = 1_234.56_78;

            // hexadecimal notation
            var num = 0x01_00;

            // binary notation
            var num2 = 0b1_0000_0000;

            balance += 227_652;

            Console.WriteLine(1_234);
        }

        public void NonCompliantCases()
        {
            int million = 1_000_00_000;  // Noncompliant {{Review this number; its irregular pattern indicates an error.}}
//                        ^^^^^^^^^^^^
        }
    }
}
