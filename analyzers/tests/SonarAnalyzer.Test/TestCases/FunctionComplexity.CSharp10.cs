public struct A
{
    const string Prefix = "_";
    const string Suffix = "_";

    private (int, int) t = default;

    public A()
    {
        int a;
        (a, var b) = t;
        const string z = $"{Prefix} zzz {Suffix}";
    }
}

public struct B
{
    const string Prefix = "_";
    const string Suffix = "_";

    private (int, int) t = default;

    public B() // Noncompliant
               // Secondary@-1
    {
        int a;
        if (false) // Secondary
        {
            (a, var b) = t;
            const string z = $"{Prefix} zzz {Suffix}";
        }
    }
}
