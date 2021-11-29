using System;

public class Sample
{
    public void Examples()
    {
        int? nullable;

        (nullable, var a) = (null, 42);
        var v = nullable.Value; // FN

        nullable = null;
        v = nullable.Value; // Noncompliant
    }
}
