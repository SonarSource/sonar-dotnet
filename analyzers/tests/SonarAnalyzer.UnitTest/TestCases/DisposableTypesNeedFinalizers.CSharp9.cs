using System;
using System.Runtime.InteropServices;

public record Foo : IDisposable // Noncompliant {{Implement a finalizer that calls your 'Dispose' method.}}
{
    private IntPtr myResource;
    private bool disposed = false;

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
    }
}

public record Bar : IDisposable // Compliant
{
    private IntPtr myResource;
    private bool disposed = false;

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
    }

    ~Bar()
    {
        Dispose(false);
    }
}
