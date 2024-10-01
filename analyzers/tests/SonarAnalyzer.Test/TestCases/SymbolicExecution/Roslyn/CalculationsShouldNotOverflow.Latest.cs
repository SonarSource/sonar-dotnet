using System;

public class Sample
{
    public void ListPattern(int[] array)
    {
        if (array is [2147483600, ..])
            _ = array[0] + 100; // FN
    }

    public void TupleDeconstruction()
    {
        int i = 2147483600;
        (i, int j) = (i + 100, 100);    // Noncompliant
    }

    public void ExtendedPropertyPattern(Type t)
    {
        if (t is { Name.Length: > 2147483600 })
            _ = t.Name.Length + 100;    // FN
    }

    public void BigMulExamples()
    {
        int a = 100000;
        int b = 100000;
        ulong aUlong = 100000;
        ulong bUlong = 100000;
        long aLong = 100000;
        long bLong = 100000;
        uint aUint = 100000;
        uint bUint = 100000;
        int resultCast = (int)Math.BigMul(a, b); // Compliant: the method will never overflow because it's returning a long and the rule doesn't report on cast operations.
        long resultLong = Math.BigMul(a, b); // Compliant
        UInt128 resultUInt128 = Math.BigMul(aUlong, bUlong); // Compliant
        Int128 resultInt128 = Math.BigMul(aLong, bLong); // Compliant
        ulong resultUlong = Math.BigMul(aUint, bUint); // Compliant
        ulong low;
        ulong high = Math.BigMul(aUlong, bUlong, out low); // Compliant
    }
}

public struct ParameterlessConstructorAndFieldInitializer
{
    int i;
    int f = 2147483600;
    public int Id { get; set; } = -2147483600;

    public ParameterlessConstructorAndFieldInitializer()
    {
        i = 2147483600;
    }

    public void PositiveOverflow()
    {
        i += 100; // FN
        f += 100; // FN
    }

    public void NegativeOverflow()
    {
        Id -= 100; // FN
    }
}
