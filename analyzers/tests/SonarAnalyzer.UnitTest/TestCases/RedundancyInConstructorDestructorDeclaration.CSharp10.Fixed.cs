using System;

record struct StaticCtor
{
    static StaticCtor() { } // Fixed
}

record struct RecordStruct
{
    public RecordStruct() { } // Fixed
}

record struct PositionalRecordStruct(string Property)
{
    public PositionalRecordStruct() : this("SomeString") { } // Compliant
}

struct Struct
{
    public Struct() { } // Fixed
}

record RecordBase
{
    public RecordBase() { }
    public RecordBase(int i) { }
}

record Record : RecordBase
{
    public Record()
{ } // Fixed

    public Record(string s) { }
}
