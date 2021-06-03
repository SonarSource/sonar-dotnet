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
        static void Main(string[] args)
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
            resultLong = bitMaskLong & - - -+1L; // Noncompliant
            resultLong = bitMaskLong & 0L; // Compliant
            resultLong = bitMaskLong | 0U; // Noncompliant
            resultLong = bitMaskLong | 0x0L; // Noncompliant
            resultLong = bitMaskLong & returnLong(); // Compliant
            resultLong = bitMaskLong & 0x0F; // Compliant

            var resultULong = 1UL | 0x00000UL; // Noncompliant
            resultULong = 1UL | 18446744073709551615UL; // Compliant

            MyMethod(1UL | 0x00000UL); // Noncompliant

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

    public void FindConstant_For_AssignedInsideLoop()
    {
        var value = 1;
        int result;
        for (var v = 0; v < 42; v++)
        {
            value = 0;
            result = value | v;     // Noncompliant
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
            result = unchanged | v; // Noncompliant
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
            result = unchanged | v; // Noncompliant
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
            result = unchanged | v; // Noncompliant
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
            result = unchanged | v; // Noncompliant
            value = 1;
            index++;
        } while (index < values.Length);
        unchanged = 1;
    }
}
