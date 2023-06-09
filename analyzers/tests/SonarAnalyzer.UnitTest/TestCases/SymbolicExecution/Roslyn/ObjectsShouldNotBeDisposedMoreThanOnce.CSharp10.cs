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
