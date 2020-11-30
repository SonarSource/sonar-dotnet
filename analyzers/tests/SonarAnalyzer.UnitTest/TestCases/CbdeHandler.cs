using System;

namespace Tests.Diagnostics
{
    public class Sample
    {
        public void PositiveOverflow() {
            int i = 2147483600;
            i +=100; // Noncompliant {{There is a path on which this operation always overflows}}
        }

        public void NegativeOverflow() {
            int i = -2147483600;
            i -=100; // Noncompliant {{There is a path on which this operation always overflows}}
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
