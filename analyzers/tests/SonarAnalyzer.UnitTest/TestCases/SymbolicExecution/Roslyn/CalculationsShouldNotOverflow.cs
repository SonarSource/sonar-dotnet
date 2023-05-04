using System;

namespace Tests.Diagnostics
{
    public class Sample
    {
        public void Types()
        {
            int i = int.MaxValue;
            _ = i + 1;              // FIXME Non-compliant
            i = int.MinValue;
            _ = i - 1;              // FIXME Non-compliant

            uint ui = uint.MaxValue;
            _ = ui + 1;             // FIXME Non-compliant
            ui = uint.MinValue;
            _ = ui - 1;             // FIXME Non-compliant

            short i16 = short.MaxValue;
            _ = i16 + 1;            // FIXME Non-compliant
            i16 = short.MinValue;
            _ = i16 - 1;            // FIXME Non-compliant

            ushort ui16 = ushort.MaxValue;
            _ = ui16 + 1;           // FIXME Non-compliant
            ui16 = ushort.MinValue;
            _ = ui16 - 1;           // FIXME Non-compliant

            long i64 = long.MaxValue;
            _ = i64 + 1;            // FIXME Non-compliant
            i64 = long.MinValue;
            _ = i64 - 1;            // FIXME Non-compliant

            ulong ui64 = ulong.MaxValue;
            _ = ui64 + 1;           // FIXME Non-compliant
            ui64 = ulong.MinValue;
            _ = ui64 - 1;           // FIXME Non-compliant

            int b = byte.MaxValue;
            _ = b + 1;              // FIXME Non-compliant
            b = byte.MinValue;
            _ = b - 1;              // FIXME Non-compliant

            sbyte sb = sbyte.MaxValue;
            _ = sb + 1;             // FIXME Non-compliant
            sb = sbyte.MinValue;
            _ = sb - 1;             // FIXME Non-compliant
        }

        public void BasicOperators()
        {
            int i = 2147483600;
            _ = i + 100;            // FIXME Non-compliant
            i = -2147483600;
            _ = i - 100;            // FIXME Non-compliant
            i = 2147483600;
            _ = i * 100;            // FIXME Non-compliant
            _ = 2147483600 << 1;    // Compliant
            _ = -2147483600 << 1;   // Compliant
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
            i <<= 1;                // Compliant
            i = -2147483600;
            i <<= 1;                // Compliant
        }

        public void Branching(int i)
        {
            if (i <= 2147483547)
                _ = i + 100;        // Compliant
            else
                _ = i + 100;        // FIXME Non-compliant

            for (i = 0; i <= 2147483547; i++)
                _ = i + 100;        // Compliant
            for (i = 0; i <= 2147483547; i++)
                _ = i + 101;        // FN symbolic execution engine is not looping that often
            for (i = 2147483546; i <= 2147483547; i++)
                _ = i + 100;        // Compliant
            for (i = 2147483546; i <= 2147483547; i++)
                _ = i + 101;        // FIXME Non-compliant

            switch (i)
            {
                case 2147483547:
                    _ = i + 100;    // Compliant
                    break;
                default:
                    _ = i + 100;    // FIXME Non-compliant
                    break;
            }
        }

        public void Lambda()
        {
            Action a = () =>
            {
                int i = -2147483600;
                i -= 100; // FN, lambdas are not supported
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
                i += 100; // FN, properties are not supported
                return i;
            }
            set
            {
                int i = 2147483600;
                i += 100; // FN, properties are not supported
            }
        }
    }

    class DotnetOverflow    // https://github.com/SonarSource/dotnet-overflow/blob/master/ApplicationWithOverflow/Program.cs
    {
        class A
        {
            public int ovflw1()
            {
                unchecked
                {
                    int i = 1834567890 + 1834567890;    // FIXME Non-compliant
                    return i;
                }
            }

            public int ovflw2()
            {
                int i = 1834567890;
                i += i;                                 // FIXME Non-compliant
                return i;
            }

            public int ovflw3()
            {
                int i = 1834567890;
                var j = i + i;                          // FIXME Non-compliant
                return j;
            }

            public int ovflw4()
            {
                int i = -1834567890;
                int j = 1834567890;
                var k = i - j;                          // FIXME Non-compliant
                return k;
            }

            public int ovflw5(int i)
            {
                if (i > 1834567890)
                {
                    return i + i;                       // FIXME Non-compliant
                }
                return 0;
            }
        }
    }
}
