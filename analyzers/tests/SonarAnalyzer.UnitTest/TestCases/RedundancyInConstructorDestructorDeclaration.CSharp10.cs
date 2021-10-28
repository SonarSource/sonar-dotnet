using System;

record struct StaticCtor
{
    static StaticCtor() { } // Noncompliant
}

record struct RecordStruct
{
    public RecordStruct() { } // Noncompliant
}

record struct PositionalRecordStruct(string Property)
{
    public PositionalRecordStruct() : this("SomeString") { } // Compliant
}

struct Struct
{
    public Struct() { } // Noncompliant
}

record RecordBase
{
    public RecordBase() { }
    public RecordBase(int i) { }
}

record Record : RecordBase
{
    public Record()
        : base() { } // Noncompliant

    public Record(string s) { }
}
