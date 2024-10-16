using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

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

public class CSharp13
{
    public class TimerRemaining
    {
        TimerRemaining countdown = new TimerRemaining()
        {
            buffer =
        {
            [^(42 | 0)] = 1, // Noncompliant
            [^(42 & -1)] = 2, // Noncompliant
            [^(42 ^ 0)] = 3, // Noncompliant
        }
        };
        public int[] buffer = new int[10];
    }
}
