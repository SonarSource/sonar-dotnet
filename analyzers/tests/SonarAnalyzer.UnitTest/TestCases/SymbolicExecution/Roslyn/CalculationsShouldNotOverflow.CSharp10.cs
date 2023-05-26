using System;

public class Sample
{
    public void TupleDeconstruction()
    {
        int i = 2147483600;
        (i, int j) = (i + 100, 100);    // Noncompliant
    }

    public void ExtendedPropertyPattern(Type t)
    {
        if (t is { Name.Length: > 2147483600 })
            _ = t.Name.Length + 100;    // FN
    }
}

public struct ParameterlessConstructorAndFieldInitializer
{
    int i;
    int f = 2147483600;
    public int Id { get; set; } = -2147483600;

    public ParameterlessConstructorAndFieldInitializer()
    {
        i = 2147483600;
    }

    public void PositiveOverflow()
    {
        i += 100; // FN
        f += 100; // FN
    }

    public void NegativeOverflow()
    {
        Id -= 100; // FN
    }
}
