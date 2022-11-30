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

    public class Comparison : IComparisonOperators<Comparison, Comparison, Comparison>
    {
        public static Comparison operator ==(Comparison? left, Comparison? right)
//                                        ^^
        {
            return left == right;
        }

        public static Comparison operator !=(Comparison left, Comparison right) => left == right; // Compliant (we do not support cross-method analysis)

        public static Comparison operator <(Comparison left, Comparison right) =>
            left is null ? left < right : left; // Compliant

        public static Comparison operator >(Comparison left, Comparison right) =>
            (left > right) is null ? left : right; // Noncompliant@-1

        public static Comparison operator <=(Comparison left, Comparison right) => left <= right; // Noncompliant

        public static Comparison operator >=(Comparison left, Comparison right) => left >= right; // Noncompliant
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

    public class BitWise : IBitwiseOperators<BitWise, BitWise, BitWise>
    {
        public static BitWise operator ~(BitWise value) => ~value; // Noncompliant
//                                     ^

        public static BitWise operator &(BitWise left, BitWise right) => left & right; // Noncompliant
        
        public static BitWise operator |(BitWise left, BitWise right) => left | right; // Noncompliant

        public static BitWise operator ^(BitWise left, BitWise right) => left ^ right; // Noncompliant
    }
}
