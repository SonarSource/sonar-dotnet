using System;
using System.Collections.Generic;

var topLevel = new List<int>();
topLevel.Clear();   // FN

topLevel.Add(42);
topLevel.Clear();

void TopLevelLocalFunction()
{
    var list = new List<int>();
    list.Clear();   // FN

    list.Add(42);
    list.Clear();
}

public class Tests
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
        list.Clear();   // Compliant
    }

    public void StaticLambda()
    {
        Action a = static () =>
        {
            var list = new List<int>();
            list.Clear();   // FIXME Non-compliant
        };
        a();
    }

    public int Property
    {
        get => 42;
        init
        {
            var list = new List<int>();
            list.Clear();   // FIXME Non-compliant
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
        collection2.Clear();    // Non-compliant
    }
}

public record Record
{
    public void Method()
    {
        var list = new List<int>();
        list.Clear();       // FIXME Non-compliant
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
        list.Clear();       // FIXME Non-compliant
    }
}
