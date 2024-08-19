class Test
{
    readonly ARecord readonlyField = new();
    ARecord readWriteField = new();

    static readonly ARecord staticReadonlyField = new();
    static ARecord staticReadWriteField = new();

    void OnAFieldOfTypeRecord()
    {
        lock (readonlyField) { }
        lock (readWriteField) { }       // Noncompliant
        lock (staticReadonlyField) { }
        lock (staticReadWriteField) { } // Noncompliant
    }

    void OnANewRecordInstance()
    {
        lock (new ARecord()) { }  // Noncompliant
    }

    record ARecord();
}
