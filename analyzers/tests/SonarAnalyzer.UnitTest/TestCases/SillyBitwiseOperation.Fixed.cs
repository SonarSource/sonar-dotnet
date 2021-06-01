using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public enum Types
    {
        Class = 0,
        Struct = 1,
        Public = 2,
        Private = 4
    }

    class SillyBitwiseOperation
    {
        static void Main(string[] args  )
        {
            int result;
            int bitMask = 0x010F;

            result = bitMask; // Fixed
            result = bitMask;  // Fixed
            result = bitMask;  // Fixed
            result = bitMask;  // Fixed
            var result2 = result;  // Fixed

            result = bitMask & 1; // Compliant
            result = bitMask | 1; // compliant
            result = bitMask ^ 1; // Compliant
            result &= 1; // Compliant
            result |= 1; // compliant
            result ^= 1; // Compliant

            long bitMaskLong = 0x010F;
            long resultLong;
            resultLong = bitMaskLong; // Fixed
            resultLong = bitMaskLong & 0L; // Compliant
            resultLong = bitMaskLong; // Fixed
            resultLong = bitMaskLong; // Fixed
            resultLong = bitMaskLong & returnLong(); // Compliant
            resultLong = bitMaskLong & 0x0F; // Compliant

            var resultULong = 1UL; // Fixed
            resultULong = 1UL | 18446744073709551615UL; // Compliant

            MyMethod(1UL); // Fixed

            var flags = Types.Class | Types.Private; // Compliant even when Class is zero
            flags = Types.Class;
            flags = flags | Types.Private;  // Compliant, even when "flags" was initally zero
        }

        private static long returnLong()
        {
            return 1L;
        }

        private static void MyMethod(UInt64 u) { }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4399
public class Repro_4399
{
    public void BuildMask(IEnumerable<DayOfWeek> daysOfWeek)
    {
        var value = 0;
        foreach (var dow in daysOfWeek)
        {
            value = value | (1 << (int)dow); // Compliant, value changes over iterations
        }
    }

    public void Repro(object[] args)
    {
        var fail = false;
        foreach (var arg in args)
        {
            fail = fail | !CheckArg(arg);   // Compliant, using || would change the logic.
        }
    }

    private bool CheckArg(object arg) => false;
}
