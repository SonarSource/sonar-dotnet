using System;
using System.Numerics;

class GenericMathFeatures
{
    void UnsignedRightShiftOperator()
    {
        int i = 1 >>> 1;
        i = 1 >>> 0x1;
        i = 2 >>> 2;
    }

    void OverloadableOperators()
    {
        var test1 = new MyClass<int>(3, 3);
        var testSub = test1 - test1; // Noncompliant
        // Secondary@-1
    }
}

public record MyClass<T>(T X, T Y) where T : ISubtractionOperators<T, T, T>
{
    public static MyClass<T> operator -(MyClass<T> left, MyClass<T> right) =>
        left with
        {
            X = left.X - right.X,
            Y = left.Y - right.Y
        };
}

public class OverriddenCompoundAssignment
{
    void Test()
    {
        var a = new C1();
        a -= a;         // Noncompliant
        // Secondary@-1

        var b = a | a; // Noncompliant
        // Secondary@-1
    }

    class C1
    {
        public int Value;

        public void operator -=(C1 x)
        {
            Value += x.Value;
        }

        public static C1 operator |(C1 a, C1 b)
        {
            return new C1 { Value = a.Value + b.Value };
        }
    }
}
