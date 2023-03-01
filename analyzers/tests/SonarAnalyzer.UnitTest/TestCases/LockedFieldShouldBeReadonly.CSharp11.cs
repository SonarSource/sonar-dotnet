using System;

class Test
{
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
