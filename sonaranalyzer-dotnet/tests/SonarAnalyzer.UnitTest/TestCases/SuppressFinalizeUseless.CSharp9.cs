using System;

GC.SuppressFinalize(new object()); // Compliant - class is static. Should we raise if `this` is not passed as parameter?

static void Foo()
{
    // There is also CA1816
    GC.SuppressFinalize(new object()); // Compliant - class is static
}

sealed record Noncompliant
{
    public void M() => GC.SuppressFinalize(this); // Noncompliant
}

record Compliant
{
    public void M() => GC.SuppressFinalize(this);

    ~Compliant() { }
}
