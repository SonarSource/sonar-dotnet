public interface Interface
{
    int Add<T>(int a, int b); // Compliant
}

public record struct R : Interface
{
    public int Add<T>(int a, int b) // Compliant, interface implementation
    {
        return 0;
    }

    T Return<T>(T v) => v; // Compliant

    int Return<T>(int v) => v; // Noncompliant

    public V DoStuff<T, V>(params V[] o) // Noncompliant
    {
        return o[0];
    }
}

public record struct R<T> // Noncompliant {{'T' is not used in the record.}}
{
}

public record struct R2<T>(T X) // Compliant
{
}

public record struct R3<T>(int X) // Noncompliant
{
}
