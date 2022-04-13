using System;
using System.Linq;
using AliasName = System.Exception;

public class Sample
{
    int field;

    public Sample() { }

    public void Go(int value)
    {
        var g = new Generic<int>();
        var alias = new AliasName();
        dynamic d = g;
        value = 42;

        var y = string.Empty;
        (y, var z) = ("a", 'x');

        foreach (var var in Enumerable.Range(0, 10)) { }
    }

    public void ValueAsVariableForCoverage()
    {
        string value = null;
    }

    public int Property
    {
        get => field;
        set
        {
            field = value;
        }
    }
}

public class Generic<TItem> { }
