using System;

void Dispose() { } // top level local function, compliant

public record R
{
    void Dispose() { } //Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
}

public record GarbageDisposalExceptionBase : IDisposable
{
    protected virtual void Dispose(bool disposing)
    {
        //...
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public partial record MyPartial
{
    public partial void Dispose();   // Noncompliant
}

public partial record MyPartial
{
    public partial void Dispose() { } // Secondary
}

public partial record MyPartial2 : IDisposable
{
    public partial void Dispose();
}

public partial record MyPartial2 : IDisposable
{
    public partial void Dispose() { }
}

public partial record MyPartial3
{
    public partial void Dispose();   // Noncompliant [another-file]
}

public ref partial struct RefStruct
{
    public partial void Dispose(); // ok
}

public ref partial struct RefStruct
{
    public partial void Dispose() { } // ok
}
