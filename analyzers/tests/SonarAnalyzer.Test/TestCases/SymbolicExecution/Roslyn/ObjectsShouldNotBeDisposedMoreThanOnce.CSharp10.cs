using System;

public record struct RecordStruct : IDisposable
{
    public void Dispose() { }

    public static void DisposeRecord()
    {
        var r = new RecordStruct();
        r.Dispose();
        r.Dispose();    // Noncompliant
    }
}

public record class RecordClass : IDisposable
{
    public void Dispose() { }

    public static void DisposeRecord()
    {
        var r = new RecordClass();
        r.Dispose();
        r.Dispose();    // Noncompliant
    }
}

public ref struct RefStruct // You can define a disposable ref struct without implementing IDisposable
                            // see docs: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct
{
    public void Dispose() { }

    public void DisposeRefStruct()
    {
        var r = new RefStruct();
        r.Dispose();
        r.Dispose(); // Noncompliant
    }

    public void DisposeWithUsing()
    {
        using (var r = new RefStruct()) // Noncompliant
        {
            r.Dispose();
        }
    }
}
