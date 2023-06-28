using System;
using System.Collections.Generic;

var topLevel = new List<int>();
topLevel.Clear();   // Noncompliant

topLevel.Add(42);
topLevel.Clear();

void TopLevelLocalFunction()
{
    var local = new List<int>();
    local.Clear();   // Noncompliant

    local.Add(42);
    local.Clear();
}

public class Sample
{
    public void TargetTypedNew()
    {
        List<int> list;

        list = new();
        list.Clear();   // Noncompliant
        list.Add(42);
        list.Clear();   // Compliant

        list = new(42);
        list.Clear();   // Noncompliant
        list.Add(42);
        list.Clear();   // Compliant

        list = new(new[] { 42 });
        list.Clear();   // Compliant

        list = new List<int>();
        list.Clear();   // Noncompliant
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            var list = new List<int>();
            list.Clear();   // Noncompliant
        };
        a();
    }

    public int Property
    {
        get => 42;
        init
        {
            var list = new List<int>();
            list.Clear();   // Noncompliant
        }
    }

    public void TargetTypedConditional(bool condition)
    {
        var filled = new List<int>() { 1 };
        var empty = new LinkedList<int>();
        ICollection<int> collection = condition ? filled : empty;
        collection.Clear();     // Compliant

        var empty2 = new List<int>();
        ICollection<int> collection2 = condition ? empty : empty2;
        collection2.Clear();    // FN
    }
}

public record Record
{
    public void Method()
    {
        var list = new List<int>();
        list.Clear();       // Noncompliant
    }
}

public partial class Partial
{
    public partial void Method();
}

public partial class Partial
{
    public partial void Method()
    {
        var list = new List<int>();
        list.Clear();       // Noncompliant
    }
}
