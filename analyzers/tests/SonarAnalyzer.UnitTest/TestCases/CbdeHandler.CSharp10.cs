using System;

namespace Tests.Diagnostics
{
    public class Sample
    {
        public void PositiveOverflow()
        {
            int i = 2147483600;
            (i, int j) = (i + 100, 100); // FN
        }

        public void NegativeOverflow()
        {
            int i = -2147483600;
            (i, int j) = (i - 100, 100); // FN
        }
    }

    public struct ParameterlessConstructorAndFieldInitializer
    {
        int i;
        int f = 2147483600;
        int k = 2147483600;
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
}
