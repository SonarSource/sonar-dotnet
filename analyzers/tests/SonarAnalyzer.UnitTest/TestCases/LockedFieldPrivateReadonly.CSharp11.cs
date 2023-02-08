using System;

class Test
{
    static readonly object staticReadonlyField = null;
    static object staticReadWriteField = null;

    readonly object readonlyField = null;
    object readWriteField = null;

    void OnANewInstance()
    {
        lock ("""a raw string literal""") { }                    // Noncompliant
        lock ($"""an interpolated {"raw string literal"}""") { } // Noncompliant
    }

    void TargetTypedObjectCreation()
    {
        lock (new() as Tuple<int>) { }                           // Error [CS8754]
    }
}
