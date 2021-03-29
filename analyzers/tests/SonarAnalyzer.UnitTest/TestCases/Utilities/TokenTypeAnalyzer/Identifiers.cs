using AliasName = System.Exception;

public class Sample
{
    int field;

    public Sample() { }

    public void Go()
    {
        var var = 42; // Second var is a contextual keyword
        var g = new Generic<int>();
        var alias = new AliasName();
        dynamic d = g;
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
