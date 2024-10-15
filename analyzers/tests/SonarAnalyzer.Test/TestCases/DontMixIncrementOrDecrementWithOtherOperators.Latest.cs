using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            nint val1 = 0;
            nint val2 = 0;

            var result = ++val1 + val2; // Noncompliant {{Extract this increment operation into a dedicated statement.}}
//                       ^^
            result = val1++ - 1; // Noncompliant
            result = 2 * val2++; // Noncompliant
            result = val2++ / 4; // Noncompliant
            result = --val1 % 2; // Noncompliant {{Extract this decrement operation into a dedicated statement.}}

            result = ++val1 * ++val2;
//                   ^^ Noncompliant
//                            ^^ Noncompliant@-1

            result = (++val2) + 1; // Noncompliant - even with parenthesis

            var text = "issue on line " + val1++ + " not expected."; // Noncompliant - even on string concat

            val1++;
            val2--;
            var res = val1 + val2;
            var other = val2 / 4;

            nuint val3 = 0;
            var result2 = (++val3) + 1; // Noncompliant
        }

        public void Foo2()
        {
            int a;
            int b = 10;
            (a, int c) = (1, 2 + b++); // Noncompliant
        }

        public void IntPtrArithmeticOperations()
        {
            IntPtr val1 = 0;
            IntPtr val2 = 0;

            var result = ++val1 + val2; // Noncompliant {{Extract this increment operation into a dedicated statement.}}
//                       ^^
            result = val1++ - 1; // Noncompliant
            result = 2 * val2++; // Noncompliant
            result = val2++ / 4; // Noncompliant
            result = --val1 % 2; // Noncompliant {{Extract this decrement operation into a dedicated statement.}}

            result = ++val1 * ++val2;
//                   ^^ Noncompliant
//                            ^^ Noncompliant@-1

            result = (++val2) + 1; // Noncompliant - even with parenthesis

            var text = "issue on line " + val1++ + " not expected."; // Noncompliant - even on string concat

            val1++;
            val2--;
            var res = val1 + val2;
            var other = val2 / 4;

            nuint val3 = 0;
            var result2 = (++val3) + 1; // Noncompliant
        }

        public void UnsignedIntPtrArithmeticOperations()
        {
            UIntPtr val1 = 0;
            UIntPtr val2 = 0;

            var result = ++val1 + val2; // Noncompliant {{Extract this increment operation into a dedicated statement.}}
//                       ^^
            result = val1++ - 1; // Noncompliant
            result = 2 * val2++; // Noncompliant
            result = val2++ / 4; // Noncompliant
            result = --val1 % 2; // Noncompliant {{Extract this decrement operation into a dedicated statement.}}

            result = ++val1 * ++val2;
//                   ^^ Noncompliant
//                            ^^ Noncompliant@-1

            result = (++val2) + 1; // Noncompliant - even with parenthesis

            var text = "issue on line " + val1++ + " not expected."; // Noncompliant - even on string concat

            val1++;
            val2--;
            var res = val1 + val2;
            var other = val2 / 4;

            nuint val3 = 0;
            var result2 = (++val3) + 1; // Noncompliant
        }

        // https://sonarsource.atlassian.net/browse/NET-487
        public void ObjectInitializer()
        {
            var i = 42;
            DataBuffer buffer = new()
            {
                [i++] = 1, // FN
                [++i] = 2, // FN

                [i--] = 3, // FN
                [--i] = 4, // FN
            };
        }
    }

    public class DataBuffer
    {
        public int this[Index index]
        {
            get => 1;
            set { /* not relevant */ }
        }
    }
}
