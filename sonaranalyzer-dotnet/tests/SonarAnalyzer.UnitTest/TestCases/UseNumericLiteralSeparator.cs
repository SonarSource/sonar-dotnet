using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void Invalid()
        {
            int i = 10000000;  // Noncompliant; is this 10 million or 100 million?
            int j = 0b01101001010011011110010101011110;  // Noncompliant {{Add underscores to this numeric value for readability.}}
//                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            long l = 0x7fffffffffffffffL;  // Noncompliant
        }

        void Valid()
        {
            int i = 10_000_000;
            int j = 0b01101001_01001101_11100101_01011110;
            long k = 0x7fff_ffff_ffff_ffffL;
            var l = 1000;
            var m = 0b001;
            var n = 0x000;
        }
    }
}
