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

// NET-3284: https://sonarsource.atlassian.net/browse/NET-3284
public class RedundantJumpStatement_C9Patterns
{
    void AllPatterns_C9Patterns(int[] numbers, int[][] arrays)
    {
        foreach (var n in numbers)
        {
            if (n is 1 or 2) continue;             // FN - OrPattern
            else if (n is > 100) continue;         // FN - RelationalPattern
            else if (n is > 0 and < 100) continue; // FN - AndPattern
            else if (n is not 100) continue;       // FN - NotPattern
        }
        foreach (var arr in arrays)
        {
            if (arr is []) continue;               // FN - ListPattern
        }
    }
}


