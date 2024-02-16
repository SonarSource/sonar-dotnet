using System;

public class Sample
{
    public void Types()
    {
        sbyte sb = sbyte.MaxValue;
        sb++;                   // Noncompliant {{This calculation is guaranteed to overflow the maximum value of '127'.}}
//      ^^^^
        sb = sbyte.MinValue;
        sb--;                   // Noncompliant {{This calculation is guaranteed to underflow the minimum value of '-128'.}}

        byte b = byte.MaxValue;
        b++;                    // Noncompliant
        b = byte.MinValue;
        b--;                    // Noncompliant

        short i16 = short.MaxValue;
        i16++;                  // Noncompliant
        i16 = short.MinValue;
        i16--;                  // Noncompliant

        ushort ui16 = ushort.MaxValue;
        ui16++;                 // Noncompliant
        ui16 = ushort.MinValue;
        ui16--;                 // Noncompliant

        int i = int.MaxValue;
        i++;                    // Noncompliant
        i = int.MinValue;
        i--;                    // Noncompliant

        uint ui = uint.MaxValue;
        ui++;                   // Noncompliant
        ui = uint.MinValue;
        ui--;                   // Noncompliant

        long i64 = long.MaxValue;
        i64++;                  // Noncompliant
        i64 = long.MinValue;
        i64--;                  // Noncompliant

        ulong ui64 = ulong.MaxValue;
        ui64++;                 // Noncompliant
        ui64 = ulong.MinValue;
        ui64--;                 // Noncompliant
    }

    public void Upcast()
    {
        sbyte sb = sbyte.MaxValue;
        _ = sb + 1;             // Compliant, upcast to int
        sb = sbyte.MinValue;
        _ = sb - 1;             // Compliant, upcast to int

        byte b = byte.MaxValue;
        _ = b + 1;              // Compliant, upcast to int
        b = byte.MinValue;
        _ = b - 1;              // Compliant, upcast to int

        short i16 = short.MaxValue;
        _ = i16 + 1;            // Compliant, upcast to int
        i16 = short.MinValue;
        _ = i16 - 1;            // Compliant, upcast to int

        ushort ui16 = ushort.MaxValue;
        _ = ui16 + 1;           // Compliant, upcast to int
        ui16 = ushort.MinValue;
        _ = ui16 - 1;           // Compliant, upcast to int

        int i = int.MaxValue;
        _ = i + 1;              // Noncompliant
        i = int.MinValue;
        _ = i - 1;              // Noncompliant

        uint ui = uint.MaxValue;
        _ = ui + 1;             // Noncompliant
        ui = uint.MinValue;
        _ = ui - 1;             // Noncompliant

        long i64 = long.MaxValue;
        _ = i64 + 1;            // Noncompliant
        i64 = long.MinValue;
        _ = i64 - 1;            // Noncompliant

        ulong ui64 = ulong.MaxValue;
        _ = ui64 + 1;           // Noncompliant
        ui64 = ulong.MinValue;
        _ = ui64 - 1;           // Noncompliant
    }

    public void BasicOperators()
    {
        int i = 2147483600;
        _ = i + 100;            // Noncompliant {{This calculation is guaranteed to overflow the maximum value of '2147483647'.}}
//          ^^^^^^^

        i = -2147483600;
        _ = i - 100;            // Noncompliant {{This calculation is guaranteed to underflow the minimum value of '-2147483648'.}}

        i = 2147483600;
        _ = i * 100;            // Noncompliant

        var j = 10;
        i = 2147483600 / j;
        _ = i * 100;            // Noncompliant

        _ = 2147483600 << 16;   // Compliant
        _ = -2147483600 << 16;  // Compliant

        i = 2 & j;
        _ = i * 2147483600;     // Noncompliant

        i = 2 | j;
        _ = i * 2147483600;     // Noncompliant

        i = 2 ^ j;
        _ = i * 2147483600;     // Noncompliant

        i = 2 % j;
        _ = i * 2147483600;     // Noncompliant
    }

    public void AssignmentOperators()
    {
        int i = 2147483600;
        i += 100;               // Noncompliant

        i = -2147483600;
        i -= 100;               // Noncompliant

        i = 2147483600;
        i *= 100;               // Noncompliant

        var j = 10;
        i = 2147483600;
        i /= j;
        _ = i * 100;            // Noncompliant

        i = 2147483600;
        i <<= 1;                // Compliant

        i = -2147483600;
        i <<= 1;                // Compliant

        i = 2;
        i &= j;
        _ = i * 2147483600;     // Noncompliant

        i = 2;
        i |= j;
        _ = i * 2147483600;     // Noncompliant

        i = 2;
        i ^= j;
        _ = i * 2147483600;     // Noncompliant

        i = 2;
        i %= j;
        _ = i * 2147483600;     // Noncompliant
    }

    public void Ranges(int i)
    {
        if (i > 2147483600)
            _ = i + 100;        // Noncompliant {{This calculation is guaranteed to overflow the maximum value of '2147483647'.}}
        if (i < 2147483600)
            _ = i + 100;        // Noncompliant {{This calculation is likely to overflow the maximum value of '2147483647'.}}
        if (i > -2147483600)
            _ = i - 100;        // Noncompliant {{This calculation is likely to underflow the minimum value of '-2147483648'.}}
        if (i < -2147483600)
            _ = i - 100;        // Noncompliant {{This calculation is guaranteed to underflow the minimum value of '-2147483648'.}}
    }

    public void Branching(int i)
    {
        if (i <= 2147483547)
            _ = i + 100;        // Compliant
        else
            _ = i + 100;        // Noncompliant

        for (i = 0; i <= 2147483547; i++)
            _ = i + 100;        // Compliant
        for (i = 0; i <= 2147483547; i++)
            _ = i + 101;        // FN
        for (i = 2147483546; i <= 2147483547; i++)
            _ = i + 100;        // Compliant
        for (i = 2147483546; i <= 2147483547; i++)
            _ = i + 101;        // Noncompliant
    }

    public void Branching2(int i)
    {
        switch (i)
        {
            case 2147483547:
                _ = i + 100;    // Compliant
                break;
            case 2147483600:
                _ = i + 100;    // Noncompliant
                break;
            default:
                _ = i + 100;    // Compliant
                break;
        }
    }

    public void Lambda()
    {
        Action a = () =>
        {
            int i = -2147483600;
            i -= 100;           // Noncompliant
        };
    }

    public int DontRaiseOnUnknownValues(int i)
    {
        _ = i + 100;
        _ = DontRaiseOnUnknownValues(i) + 100;
        return i;
    }

    public override int GetHashCode()
    {
        int i = int.MaxValue;
        return i + 100; // Compliant, we don't want to run here
    }
}

public class Properties
{
    public int GetSet
    {
        get
        {
            int i = 2147483600;
            i += 100;           // Noncompliant
            return i;
        }
        set
        {
            int i = 2147483600;
            i += 100;           // Noncompliant
        }
    }

    public void Untracked(Properties o)
    {
        if (o.GetSet == int.MaxValue)
            o.GetSet++;         // Compliant
        if (o.GetSet == int.MaxValue)
            ++o.GetSet;         // Compliant
        if (o.GetSet == int.MinValue)
            o.GetSet--;         // Compliant
        if (o.GetSet == int.MinValue)
            --o.GetSet;         // Compliant
    }
}

class DotnetOverflow
{
    public int UncheckedStatement()
    {
        int i, j = int.MaxValue;
        unchecked
        {
            i = 1834567890 + 1834567890;    // Compliant, we don't want to run for methods with unchecked
        }
        j = j + 100;    // Compliant, we don't want to run here either, because there's unchecked part in the code
        return i + j;   // Compliant
    }

    public int UncheckedExpression()
    {
        int i, j = int.MaxValue;
        i = unchecked(1834567890 + 1834567890);    // Compliant, we don't want to run for methods with unchecked
        j = j + 100;    // Compliant, we don't want to run here either, because there's unchecked part in the code
        return i + j;   // Compliant
    }

    public int Overflow2()
    {
        int i = 1834567890;
        i += i;                                 // Noncompliant
        return i;
    }

    public int Overflow3()
    {
        int i = 1834567890;
        var j = i + i;                          // Noncompliant
        return j;
    }

    public int Overflow4()
    {
        int i = -1834567890;
        int j = 1834567890;
        var k = i - j;                          // Noncompliant
        return k;
    }

    public int Overflow5(int i)
    {
        if (i > 1834567890)
        {
            return i + i;                       // Noncompliant
        }
        return 0;
    }
}

public class CustomOperator
{
    public void Coverage()
    {
        // To cover the `Min(ITypeSymbol)` returning `null`. The char doesn't help here, because ch+ch returns int.
        CustomOperator value = (CustomOperator)(object)42;      // Anything with NumberConstraint and + operation
        value = value + value;                                  // Compliant
    }

    public static CustomOperator operator +(CustomOperator left, CustomOperator right) => null;
}
