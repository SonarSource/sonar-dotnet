using System;

record struct StaticCtor
{
}

record struct RecordStruct
{
}

record struct PositionalRecordStruct(string Property)
{
    public PositionalRecordStruct() : this("SomeString") { } // Compliant
}

struct Struct
{
}
