using System;
using System.Collections.Generic;

public class Sample
{
    public void InvocationArgument(bool condition, object a)
    {
        Console.WriteLine(condition ? a.ToString() : "Default");
    }

    public void If(bool outer, bool inner)
    {
        if (outer)
        {
            if (inner)
            {
                var a = "Outer && Inner";
            }
            else
            {
                var a = "Outer && !Inner";
            }
        }
        else
        {
            var a = "!Outer";
        }
    }

    public void CoalesceTernaryCoalesce(bool condition, string first, string ternaryTrue, string ternaryFalse, string last)
    {
        var x = first ?? (condition ? ternaryTrue : ternaryFalse) ?? last;
    }

    public void TermaryCoalesce(bool condition, string first, string second, string ternaryFalse, string last)
    {
        var x = condition ? (first ?? second) : ternaryFalse;
    }

    public void CoalescingAssignmentTermaryCoalescingAssingment(bool condition, string first, string second, string ternaryTrue, string ternaryFalse)
    {
        first ??= second ??= (condition ? ternaryTrue : ternaryFalse);
    }

    public void ConditionalAccessCoalesce(object o)
    {
        var ret = o?.ToString() ?? "N/A";
    }

    public void ConditionalAccessChained(object o)
    {
        var ret = o?.ToString()?.Substring(0, 1)?.Length;
    }

    public void While(int i)
    {
        while (i > 0)
        {
            i--;
            var j = i;
            while (j > 0)
            {
                j--;
            }
        }
    }

    public void For(int a, int b)
    {
        for (int i = 0; i < a; i++)
        {
            for (int j = 0; j < b; i++)
            {
                var x = i + j;
            }
        }
    }

    public void ForInfinite()
    {
        for (; ; )
        {
            var value = "Value";
            break;
        }
    }

    public void ForEach(string[] names, int[] numbers)
    {
        foreach (var number in numbers)
        {
            foreach (var name in names)
            {
                var ret = $"{number}. {name}";
            }
        }
    }

    public void SwitchExpression(int a, int b)
    {
        var ret = a switch
        {
            0 => "Zero",
            1 => "One",
            2 => b switch { 0 => "Two bIsZero", 1 => "Two bIsOne", _ => "Two bIsMore" },
            3 => "Three",
            _ => null
        };
    }

    public void NullCoalesceAssignmentToElementAccess(List<string> list, string arg)
    {
        var result = list[42] ??= arg;
    }
}
