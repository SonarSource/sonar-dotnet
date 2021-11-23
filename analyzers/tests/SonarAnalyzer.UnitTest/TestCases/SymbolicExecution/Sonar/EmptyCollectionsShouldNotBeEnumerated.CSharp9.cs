using System;
using System.Collections.Generic;

var topLevel = new List<int>();
topLevel.Clear();   // FN

topLevel.Add(42);
topLevel.Clear();

void TopLevelLocalFunction()
{
    var local = new List<int>();
    local.Clear();   // FN

    local.Add(42);
    local.Clear();
}

public class Sample
{
    public void TargetTypedNew()
    {
        List<int> list;

        list = new();
        list.Clear();   // FN
        list.Add(42);
        list.Clear();

        list = new(42);
        list.Clear();   // FN
        list.Add(42);
        list.Clear();

        list = new(new[] { 42 });
        list.Clear();   //Compliant

        list = new List<int>();
        list.Clear();   // FN, can't build CFG for this method
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            var list = new List<int>();
            list.Clear();       // Noncompliant
        };
        a();
    }

    public int Property
    {
        get => 42;
        init
        {
            var list = new List<int>();
            list.Clear();       // Noncompliant
        }
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

public class TartetTypedConditional
{
    public void Go(bool condition)
    {
        var filled = new List<string>() { "a" };
        var empty = new LinkedList<string>();
        ICollection<string> collection = condition ? filled : empty;
        collection.Clear(); // FN

        var filledLinked = new LinkedList<string>();
        filledLinked.AddLast("b");
        collection = condition ? filled : filledLinked;
        collection.Clear();
    }
}

