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

    class UnnecessaryBitwiseOperation
    {
        static void Main(string[] args)
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

        public static void WithUnaryPrefix()
        {
            int result;
            int bitMask = 0x010F;
            int one = 1;
            int zero = 0;

            result = bitMask & -one;       // FN - Unary Operator is not supported
            result &= -one;                // FN - Unary Operator is not supported
            result = bitMask & - - -+one;  // FN - Unary Operator is not supported
            result = bitMask | + + +zero;  // FN - Unary Operator is not supported
            result = bitMask | + + +zero;  // FN - Unary Operator is not supported
        }

        public void UnaryOperators()
        {
            var length = 0x80;

            var bytes1 = 0;
            bytes1++;
            _ = bytes1 | 0x80; // Compliant, See: https://github.com/SonarSource/sonar-dotnet/issues/6326

            var bytes2 = 0;
            ++bytes2;
            _ = bytes2 | 0x80; // Compliant

            var bytes3 = 0;
            bytes3--;
            _ = bytes3 | 0x80; // Compliant

            var bytes4 = 0;
            --bytes4;
            _ = bytes4 | 0x80; // Compliant

            var bytes5 = 0;
            var bytesOther = 0;
            --bytesOther;      // Other variable is mutated
            _ = 0x80; // Fixed

            var bytes6 = 0;
            bytesOther++;      // Other variable is mutated
            _ = 0x80; // Fixed
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
            fail = fail | !CheckArg(arg);   // Compliant, using short-circuit operator || would change the logic.
        }
    }

    private bool CheckArg(object arg) => false;

    public void FindConstant_For_AssignedInsideLoop()
    {
        var value = 1;
        int result;
        for (var v = 0; v < 42; v++)
        {
            value = 0;
            result = v;     // Fixed
        }
    }

    public void FindConstant_For_ReassignedToTheSameValue()
    {
        var value = 0;
        int result;
        for (var v = 0; v < 42; v++)
        {
            result = value | v;     // FN per rule description, but expected behavior for ConstantValueFinder. Variable "value" is reassigned inside a loop.
            value = 0;
        }
    }

    public void FindConstant_For()
    {
        var value = 0;
        var unchanged = 0;
        int result;
        for (var v = 0; v < 42; v++)
        {
            result = value | v;     // Compliant, value changes over iterations
            result = v; // Fixed
            value = 1;
        }
    }

    public void FindConstant_ForEach(int[] values)
    {
        var value = 0;
        var unchanged = 0;
        int result;
        foreach (var v in values)
        {
            result = value | v;     // Compliant, value changes over iterations
            result = v; // Fixed
            value = 1;
        }
        unchanged = 1;
    }

    public void FindConstant_While(int[] values)
    {
        var value = 0;
        var unchanged = 0;
        int result;
        int index = 0;
        while (index < values.Length)
        {
            var v = values[index];
            result = value | v;     // Compliant, value changes over iterations
            result = v; // Fixed
            value = 1;
            index++;
        }
        unchanged = 1;
    }

    public void FindConstant_Do(int[] values)
    {
        var value = 0;
        var unchanged = 0;
        int result;
        var index = 0;
        do
        {
            var v = values[index];
            result = value | v;     // Compliant, value changes over iterations
            result = v; // Fixed
            value = 1;
            index++;
        } while (index < values.Length);
        unchanged = 1;
    }

    class LocalFields
    {
        private int start = 0;

        public int End { get; set; } = 0;

        public void UpdateStart(int val) => start = val;

        public override int GetHashCode()
        {
            var value1 = Method() ^ End;
            var value2 = start ^ End;
            var value3 = End ^ start;
            return Method() ^ start;

            int Method() => 42;
        }
    }
}
