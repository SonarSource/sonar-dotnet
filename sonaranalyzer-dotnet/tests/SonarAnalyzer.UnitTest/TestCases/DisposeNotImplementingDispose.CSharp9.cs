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

public partial record MyPartial : IDisposable
{
    public void Dispose()
    {
        // Dispose(10)
    }
}

public partial record MyPartial
{
    public void Dispose(int i) // FN, partial classes are not processed, see https://github.com/dotnet/roslyn/issues/3748
    {
    }
}

public ref partial struct RefStruct
{
    public partial void Dispose(); // ok
}

public ref partial struct RefStruct
{
    public partial void Dispose() { } // ok
}
