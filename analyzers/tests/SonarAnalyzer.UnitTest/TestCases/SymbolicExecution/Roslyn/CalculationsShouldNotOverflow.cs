using System;

public class Sample
{
    public void Types()
    {
        sbyte sb = sbyte.MaxValue;
        sb++;                   // Noncompliant
        sb = sbyte.MinValue;
        sb--;                   // Noncompliant

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
        _ = i + 100;            // Noncompliant

        i = -2147483600;
        _ = i - 100;            // Noncompliant

        i = 2147483600;
        _ = i * 100;            // FIXME Non-compliant

        i = 2147483600 / 10;
        _ = i * 100;            // FIXME Non-compliant

        _ = 2147483600 << 16;   // Compliant
        _ = -2147483600 << 16;  // Compliant

        i = 2 & 10;
        _ = i * 2147483600;     // FIXME Non-compliant

        i = 2 | 10;
        _ = i * 2147483600;     // FIXME Non-compliant

        i = 2 ^ 10;
        _ = i * 2147483600;     // FIXME Non-compliant

        i = 2 % 10;
        _ = i * 2147483600;     // FIXME Non-compliant
    }

    public void AssignmentOperators()
    {
        int i = 2147483600;
        i += 100;               // FIXME Non-compliant

        i = -2147483600;
        i -= 100;               // FIXME Non-compliant

        i = 2147483600;
        i *= 100;               // FIXME Non-compliant

        i = 2147483600;
        i /= 10;
        _ = i * 100;            // FIXME Non-compliant 

        i = 2147483600;
        i <<= 1;                // Compliant

        i = -2147483600;
        i <<= 1;                // Compliant

        i = 2;
        i &= 10;
        _ = i * 2147483600;     // FIXME Non-compliant

        i = 2;
        i |= 10;
        _ = i * 2147483600;     // FIXME Non-compliant

        i = 2;
        i ^= 10;
        _ = i * 2147483600;     // FIXME Non-compliant

        i = 2;
        i %= 10;
        _ = i * 2147483600;     // FIXME Non-compliant
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
            _ = i + 101;        // Noncompliant likely
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
            case 2147483599:
                _ = i + 100;    // FIXME Non-compliant
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
            i -= 100;           // FIXME Non-compliant
        };
    }

    public int DontRaiseOnUnknownValues(int i)
    {
        _ = i + 100;
        _ = DontRaiseOnUnknownValues(i) + 100;
        return i;
    }
}

public class Properties
{
    public int GetSet
    {
        get
        {
            int i = 2147483600;
            i += 100;           // FIXME Non-compliant
            return i;
        }
        set
        {
            int i = 2147483600;
            i += 100;           // FIXME Non-compliant
        }
    }
}

class DotnetOverflow
{
    class A
    {
        public int Overflow1()
        {
            unchecked
            {
                int i = 1834567890 + 1834567890;    // Noncompliant
                return i;
            }
        }

        public int Overflow2()
        {
            int i = 1834567890;
            i += i;                                 // FIXME Non-compliant
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
}
