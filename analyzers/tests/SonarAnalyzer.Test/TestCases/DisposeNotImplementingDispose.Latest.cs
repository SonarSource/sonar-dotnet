using System;
using System.Collections.Generic;
using System.IO;

public ref struct RefStruct
{
    public void Dispose() // ok
    {
    }
}

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

public ref partial struct PartialRefStruct
{
    public partial void Dispose(); // ok
}

public ref partial struct PartialRefStruct
{
    public partial void Dispose() { } // ok
}

public record struct Struct
{
    public void Dispose() // Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
    {
    }
}

public struct DisposableStruct : IDisposable
{
    public DisposableStruct() { }
    public void Dispose() { }
}

public record struct PositionalRecordStruct(string Value)
{
    public void Dispose() // Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
    {
    }
}

public record struct DisposablePositionalRecordStruct(string Value) : IDisposable
{
    public void Dispose() { }
}

public interface IStaticAbstractMethod
{
    static abstract void Dispose(); // Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
}

public interface IStaticVirtualMethod
{
    static virtual void Dispose() { } // Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
}

class Sample { }

static class Extensions
{
    extension(Sample s)
    {
        void Dispose() { }  // Noncompliant FP
    }
}
