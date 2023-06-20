using System;
using System.Collections.Generic;

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

        list.Clear();
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
