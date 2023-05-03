using System;

namespace Tests.Diagnostics
{
    public class Sample
    {
        public void PositiveOverflow()
        {
            int i = 2147483600;
            i += 100; // FIXME Non-compliant {{This calculation is guaranteed to overflow the maximum value of 'int'.}}
        }

        public void NegativeOverflow()
        {
            int i = -2147483600;
            i -= 100; // FIXME Non-compliant {{This calculation is guaranteed to overflow the maximum value of 'int'.}}
        }

        public void Lambda()
        {
            Action a = () =>
            {
                int i = -2147483600;
                i -= 100; // FN, lambdas are not supported
            };
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
