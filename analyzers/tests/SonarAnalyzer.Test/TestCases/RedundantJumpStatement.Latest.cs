using System;

public class RedundantJumpStatement
{
    void CSharp8_StaticLocalFunctions()
    {
        static void Compute(int a, out int b)
        {
            b = a;
            return;     // Noncompliant
        }
        static void EnsurePositive(int a, out int b)
        {
            b = 0;
            if (a <= 0)
            {
                return;
            }
            b = a;
        }
    }
}

public interface IWithDefaultImplementation
{
    decimal Count { get; set; }
    decimal Price { get; set; }

    //Default interface methods
    void Reset()
    {
        Price = 0;
        Count = 0;
        return;     // Noncompliant
    }

    void ResetIfZero()
    {
        if (Count == 0)
        {
            return;
        }
        Price = 0;
    }
}

record Record
{
    int Prop
    {
        init
        {
            goto A; // Noncompliant
        A:
            return; // Noncompliant
        }
    }
}

