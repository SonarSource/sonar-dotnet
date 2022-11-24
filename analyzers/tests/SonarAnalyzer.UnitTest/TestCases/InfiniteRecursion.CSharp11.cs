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
        public static Addition operator +(Addition left, Addition right) => left + right; // Noncompliant
//                                      ^
    }

    public class Equality : IEqualityOperators<Equality, Equality, Equality>
    {
        public static Equality operator ==(Equality left, Equality right) => left == right; // Noncompliant
//                                      ^^

        public static Equality operator !=(Equality left, Equality right) => left != right; // Noncompliant
//                                      ^^
    }

    public class Multiply : IMultiplyOperators<Multiply, Multiply, int>
    {
        public int Value { get; set; }

        public static int operator *(Multiply left, Multiply right) => left.Value * right.Value; // Compliant (not looping)
    }

    public class Decrement : IDecrementOperators<Decrement>
    {
        public static Decrement operator --(Decrement val) => --val; // Noncompliant
//                                       ^^
    }

    public class DecrementAfter : IDecrementOperators<DecrementAfter>
    {
        public static DecrementAfter operator --(DecrementAfter val) => val--; // Noncompliant
    }

    public class Increment : IIncrementOperators<Increment>
    {
        public static Increment operator ++(Increment val) => ++val; // Noncompliant
    }

    public class IncrementAfter : IIncrementOperators<IncrementAfter>
    {
        public static IncrementAfter operator ++(IncrementAfter val) => val++; // Noncompliant
    }
}
