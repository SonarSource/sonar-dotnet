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
}
