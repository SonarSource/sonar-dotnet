using System;

public class Sample
{
    public void Destructuring()
    {
        int? nullable;

        (nullable, _) = (null, 42);
        var v = nullable.Value; // FN

        nullable = null;
        v = nullable.Value;     // Noncompliant
    }
}
