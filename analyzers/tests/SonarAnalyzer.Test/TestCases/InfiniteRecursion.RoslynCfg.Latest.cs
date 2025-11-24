using System;
using System.Numerics;

// https://github.com/SonarSource/sonar-dotnet/issues/6646
namespace Repro_6646
{
    public class Repro
    {
        public string Name
        {
            init // Noncompliant
            {
                Name = value;
            }
        }

        public string Arrow
        {
            init => Arrow = value;   // Noncompliant
        }
    }
}

namespace CSharp.Thirteen
{
    //https://sonarsource.atlassian.net/browse/NET-403
    public partial class PartialProperty
    {
        public partial string Name { get; set; }
        public partial string Name2 { init; }
        public partial int Method();
    }

    public partial class PartialProperty
    {
        public partial string Name
        {
            get // Compliant FN
            {
                return Name;
            }
            set // Compliant FN
            {
                Name = value;
            }
        }
        public partial string Name2
        {
            init // Compliant FN
            {
                Name2 = value;
            }
        }
        public partial int Method()
        {
            Method(); // Compliant FN
            return 1;
        }
    }
}

namespace Tests.Diagnostics
{
    public class CSharp8
    {
        int CSharp8_SwitchExpressions_OK(int a)
        {
            return a switch
            {
                0 => 1,
                1 => 0,
                _ => CSharp8_SwitchExpressions_OK(a) % 2
            };
        }

        int CSharp8_SwitchExpressions_Bad(int a)    // Noncompliant
        {
            return a switch
            {
                0 => CSharp8_SwitchExpressions_Bad(a + 1),
                1 => CSharp8_SwitchExpressions_Bad(a - 1),
                _ => CSharp8_SwitchExpressions_Bad(a) % 2
            };
        }

        int CSharp8_StaticLocalFunctions_OK(int a)
        {
            static int Calculate(int a, int b) => a + b + 1;

            return Calculate(a, 1);
        }

        int CSharp8_StaticLocalFunctions_Bad(int a, int b)
        {
            static int Calculate(int a, int b) => Calculate(a, b) + 1;  //Noncompliant

            return Calculate(a, b);
        }

        int CSharp8_StaticLocalFunctions_FN(int a, int b)
        {
            static int Add(int a, int b) => Fix(a, b);  // FN - Two methods calling each other are not recognized
            static int Fix(int a, int b) => Add(a, b);

            return Add(a, b);
        }
    }

    public interface IWithDefaultImplementation
    {
        decimal Count { get; set; }
        decimal Price { get; set; }

        decimal Total() //Noncompliant
        {
            return Count * Price + Total();
        }
    }

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

public static class Extensions
{
    extension(Exception ex)
    {
        public void Infinite()  // Noncompliant
        {
            ex.Infinite();
        }

        public void Finite(int count)
        {
            if (count > 0)
            {
                ex.Finite(count - 1);
            }
        }

        public int InstanceInfiniteCount => ex.InstanceInfiniteCount + 1; // FN
        public int InstanceFiniteCount => ex.Message.Length + 1;

        public static int StaticInfiniteCount => Exception.StaticInfiniteCount + 1; // Noncompliant
        public static int StaticFiniteCount => 4 + 2;
    }
}
