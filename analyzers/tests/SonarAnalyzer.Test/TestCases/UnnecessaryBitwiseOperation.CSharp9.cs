using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

nint resultNint;
nint bitMaskNint = 0x010F;
const nint nOne = 1;
const nint nZero = 0;

resultNint = -1 & bitMaskNint;            // Noncompliant
resultNint = bitMaskNint & -nOne;         // Noncompliant
resultNint = bitMaskNint | nZero;         // Noncompliant
resultNint = bitMaskNint ^ nZero;         // Noncompliant
resultNint = bitMaskNint | IntPtr.Zero;   // Noncompliant
resultNint = bitMaskNint ^ IntPtr.Zero;   // Noncompliant
resultNint &= -nOne;                      // Noncompliant
resultNint |= nZero;                      // Noncompliant
resultNint ^= nZero;                      // Noncompliant
var result2 = resultNint ^= nZero;        // Noncompliant

resultNint = bitMaskNint & - - -+nOne; // Noncompliant
resultNint = bitMaskNint | + + +nZero; // Noncompliant

resultNint = bitMaskNint & 1;   // Compliant
resultNint = bitMaskNint | 1;   // Compliant
resultNint = bitMaskNint ^ 1;   // Compliant
resultNint &= 1;                // Compliant
resultNint |= 1;                // Compliant
resultNint ^= 1;                // Compliant

nuint bitMaskNuint = 0x010F;
nuint resultNuint;
const nuint nuZero = 0;

resultNuint = bitMaskNuint | + + +nuZero;   // Noncompliant
resultNuint = bitMaskNuint & nuZero;        // Compliant
resultNuint = bitMaskNuint ^ 0;             // Noncompliant
resultNuint = bitMaskNuint | 0;             // Noncompliant
resultNuint = bitMaskNuint | 0x0;           // Noncompliant
resultNuint = bitMaskNuint ^ UIntPtr.Zero;  // Noncompliant
resultNuint = bitMaskNuint | UIntPtr.Zero;  // Noncompliant
resultNuint = bitMaskNuint & returnNuint(); // Compliant
resultNuint = bitMaskNuint & 0x0F;          // Compliant

MyMethod(1 | 0x00000); // Noncompliant

static void MyMethod(nuint u) { }
static nuint returnNuint() => 1;

public class CSharp8
{
    public async Task FindConstant_AwaitForEach(IAsyncEnumerable<int> values)
    {
        var value = 0;
        var unchanged = 0;
        int result;
        await foreach (var v in values)
        {
            result = value | v;     // Compliant, value changes over iterations
            result = unchanged | v; // Noncompliant
            value = 1;
        }
        unchanged = 1;
    }

    public void TupleAssignment()
    {
        var i = 0;
        (i, _) = (1, 1);
        _ = i | 0x80;
    }

    public void InterlockedMethods()
    {
        var bytes1 = 0;
        Interlocked.Add(ref bytes1, 1);
        _ = bytes1 | 0x80; // Compliant

        var bytes2 = 0;
        Interlocked.Decrement(ref bytes2);
        _ = bytes2 | 0x80; // Compliant

        var bytes3 = 0;
        Interlocked.Increment(ref bytes3);
        _ = bytes3 | 0x80; // Compliant

        var bytes4 = 0;
        Interlocked.And(ref bytes4, 0x80);
        _ = bytes4 | 0x80; // Compliant

        var bytes5 = 0;
        Interlocked.Or(ref bytes5, 0x80);
        _ = bytes5 | 0x80; // Compliant

        var bytes6 = 0;
        Interlocked.Exchange(ref bytes6, 0x80);
        _ = bytes6 | 0x80; // Compliant

        var bytes7 = 0;
        Interlocked.CompareExchange(ref bytes7, 100, 0x80);
        _ = bytes7 | 0x80; // Compliant

        var bytes8 = 0;
        var otherBytes = 0;
        // Other variable passed
        Interlocked.Increment(ref otherBytes);
        _ = bytes8 | 0x80; // Noncompliant
    }

    public void RefOrOut()
    {
        var bytes1 = 0;
        // Not passed by ref
        _ = Convert.ToString(bytes1);
        _ = bytes1 | 0x80; // Noncompliant


        var bytes2 = 0;
        // Passed by out
        _ = int.TryParse("1", out bytes2);
        _ = bytes2 | 0x80; // Compliant

        // ref tests are in InterlockedMethods()
    }

    public unsafe void UnaryOperators()
    {
#nullable enable
        var bytes1 = 0;
        _ = bytes1!; // SuppressNullableWarning does not mutate the value
        _ = bytes1 | 0x80; // Noncompliant
#nullable disable

        var bytes2 = 0;
        _ = +bytes2; // UnaryPlus does not mutate the value
        _ = bytes2 | 0x80; // Noncompliant

        var bytes3 = 0;
        _ = -bytes3; // UnaryMinus does not mutate the value
        _ = bytes3 | 0x80; // Noncompliant

        var bytes4 = 0;
        _ = ~bytes4; // BitwiseNot does not mutate the value
        _ = bytes4 | 0x80; // Noncompliant

        // LogicalNot does not mutate and can not be tested here

        var bytes5 = 0;
        var pointer5 = &bytes5; // The address of operator indicates a possible mutation through the pointer
        _ = bytes5 | 0x80; // Compliant

        var bytes6 = 0;
        var pointer6 = &bytes6; // The address of operator indicates a possible mutation through the pointer
        *pointer6 = 1; // Pointer indirect mutates a value
        _ = bytes6 | 0x80; // Compliant
    }
}
