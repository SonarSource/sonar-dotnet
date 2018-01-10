using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{

    class SillyBitwiseOperation
    {
        static void Main(string[] args  )
        {
            int result;
            int bitMask = 0x010F;

            result = -1 & bitMask; // Noncompliant
//                   ^^^^
            result = bitMask | 0;  // Noncompliant
//                           ^^^
            result = bitMask ^ 0;  // Noncompliant
            result = bitMask ^ 0;  // Noncompliant {{Remove this silly bit operation.}}
            result &= -1; // Noncompliant
            result |= 0;  // Noncompliant
            result ^= 0;  // Noncompliant
            var result2 = result ^= 0;  // Noncompliant

            result = bitMask & 1; // Compliant
            result = bitMask | 1; // compliant
            result = bitMask ^ 1; // Compliant
            result &= 1; // Compliant
            result |= 1; // compliant
            result ^= 1; // Compliant

            long bitMaskLong = 0x010F;
            long resultLong;
            resultLong = bitMaskLong & - - - +1L; // Noncompliant
            resultLong = bitMaskLong & 0L; // Compliant
            resultLong = bitMaskLong | 0U; // Noncompliant
            resultLong = bitMaskLong | 0x0L; // Noncompliant
            resultLong = bitMaskLong & returnLong(); // Compliant
            resultLong = bitMaskLong & 0x0F; // Compliant

            var resultULong = 1UL | 0x00000UL; // Noncompliant
            resultULong = 1UL | 18446744073709551615UL; // Compliant

            m(1UL | 0x00000UL); // Noncompliant
        }
        private static long returnLong()
        {
            return 1L;
        }
    }
}
