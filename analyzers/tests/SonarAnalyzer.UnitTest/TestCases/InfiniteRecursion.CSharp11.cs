using System;
using System.Numerics;

namespace Tests.Diagnostics
{
    interface InfiniteRecursion
    {
        static virtual int Pow<T>(int num, int exponent) where T : InfiniteRecursion // Noncompliant
        {
            num *= T.Pow<T>(num, exponent - 1);
            return num;  // this is never reached
        }
    }

    public class Addition : IAdditionOperators<Addition, Addition, Addition>
    {
        public static Addition operator +(Addition left, Addition right) => left + right; // FN
    }
}
