using System;
using AliasName = System.Exception;

public class Sample
{
    int aField;

    public Sample() { }

    public void Go(int value)
    {
        var g = new Generic<int>();
        var alias = new AliasName();
        dynamic d = g;
        value = 42;

        var y = string.Empty;
        (y, var z) = ("a", 'x');
    }

    public void ValueAsVariableForCoverage()
    {
        string value = null;
    }

    public int Property
    {
        get => aField;
        set
        {
            aField = value;
        }
    }
}

public class Generic<TItem> { }
