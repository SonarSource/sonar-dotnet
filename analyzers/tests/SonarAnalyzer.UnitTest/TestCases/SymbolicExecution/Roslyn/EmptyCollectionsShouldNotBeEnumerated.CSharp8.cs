using System;
using System.Collections.Generic;

public class NewFrameworkMethods
{
    public void Methods_Raise_Issue()
    {
        int i;

        var queue = new Queue<int>();
        queue.TryDequeue(out i);        // Noncompliant
        queue.TryPeek(out i);           // Noncompliant

        var stack = new Stack<int>();
        stack.TryPeek(out i);           // Noncompliant
        stack.TryPop(out i);            // Noncompliant
    }

    public void Methods_Ignored()
    {
        var list = new List<int>();
        list.EnsureCapacity(5);

        var set = new HashSet<int>();
        set.EnsureCapacity(5);

        var queue = new Queue<int>();
        queue.EnsureCapacity(5);

        var stack = new Stack<int>();
        stack.EnsureCapacity(5);

        var dict = new Dictionary<int, int>();
        dict.EnsureCapacity(5);
        dict.TrimExcess();
    }

    public void Methods_Set_NotEmpty()
    {
        var dict = new Dictionary<int, int>();
        dict.TryAdd(1, 5);
        dict.Clear();                   // Compliant
    }
}

public class NullCoalescenceAssignment
{
    public void Test()
    {
        List<int> list = null;
        list ??= new List<int>();
        list.Clear(); // Noncompliant
    }
}

public class SwitchExpression
{
    private static bool Predicate(int i) => true;

    public bool InsideSwitch(int type)
    {
        var list = new List<int>();

        return type switch
        {
            1 => list.Exists(Predicate), // Noncompliant
            _ => false
        };
    }

    public void UsingSwitchResult(bool cond)
    {
        var list = cond switch
        {
            _ => new List<int>()
        };

        list.Clear(); // Noncompliant
    }

    public void UsingSwitchResult_Compliant(bool cond)
    {
        var list = cond switch
        {
            true => new List<int> { 5 },
            _ => new List<int>()
        };

        list.Clear();   // Compliant
    }
}

public interface IWithDefaultMembers
{
    public void Test(int a, int b) { }

    public void Test()
    {
        var list = new List<int>();
        list.Clear(); // Noncompliant
    }

    public void TestWithNestedSwitchExpression()
    {
        var list = new List<int>();
        Test(list.IndexOf(1), list.Count switch { 1 => 2, _ => 3 }); // Noncompliant
    }
}

public class LocalStaticFunctions
{
    public void Method(object arg)
    {
        void LocalFunction(object o)
        {
            var list = new List<int>();
            list.Clear(); // Noncompliant
        }

        static void LocalStaticFunction(object o)
        {
            var list = new List<int>();
            list.Clear(); // Noncompliant
        }
    }
}

public class Ranges
{
    public void Method(string[] words)
    {
        var someWords = words[1..4];
        someWords.Clone(); // Compliant

        var noWords = words[1..1];
        noWords.Clone(); // Compliant - FN, the collection is empty (https://github.com/SonarSource/sonar-dotnet/issues/2944)
    }
}

public class DeclarationExpression
{
    public void Flush(IDecoder decoder)
    {
        var first = decoder.Convert(out int bytesUsed, out int charsUsed);

        var second = decoder.Convert(out var _, out var _);

        var third = decoder.Convert(out _, out _);
    }
}

public interface IDecoder
{
    bool Convert(out int i3, out int i4);
}

// https://github.com/SonarSource/sonar-dotnet/issues/6179
public class Repro_6179
{
    public void FilledInLoop_FromAnotherCollection_FromArray()
    {
        var data = new[] { 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0 };
        var list = new List<List<int>>();
        List<int> currentBin = null;
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] == 0)
            {
                if (currentBin != null)
                {
                    list.Add(currentBin);
                    currentBin = null;
                }

                continue;
            }

            currentBin ??= new List<int>();
            currentBin.Add(i);
        }

        for (var i = 0; i < list.Count; i++)
        {
            list.ForEach(x => { }); // Compliant
        }
    }

    public void FilledInLoop_FromAnotherCollection_FromList()
    {
        var data = new List<int> { 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0 };
        var list = new List<List<int>>();
        List<int> currentBin = null;
        for (var i = 0; i < data.Count; i++)
        {
            if (data[i] == 0)
            {
                if (currentBin != null)
                {
                    list.Add(currentBin);
                    currentBin = null;
                }

                continue;
            }

            currentBin ??= new List<int>();
            currentBin.Add(i);
        }

        for (var i = 0; i < list.Count; i++)
        {
            list.ForEach(x => { }); // Compliant
        }
    }
}
