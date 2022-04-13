using System;
using System.Linq;
using AliasName = System.Exception;

public class Sample
{
    int field;

    public Sample()
    {
        var var = string.Empty;
        (var, var z) = ("a", 'x');
    }

    public void Go(int value)
    {
        var var = new Generic<int>();
        var alias = new AliasName();
        dynamic dynamic = var;
        value = 42;
    }

    public void ValueAsVariableForCoverage()
    {
        string value = null;

        foreach (var var in Enumerable.Range(0, 10)) { }
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
