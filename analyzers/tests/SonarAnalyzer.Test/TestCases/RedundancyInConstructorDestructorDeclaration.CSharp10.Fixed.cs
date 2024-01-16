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

// https://github.com/SonarSource/sonar-dotnet/issues/8087
public readonly struct Repro_8087
{
    // More info here https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/constructor-errors?f1url=%3FappId%3Droslyn%26k%3Dk(CS8983)#constructors-in-struct-types
    public Repro_8087() { } // Compliant - Would mix with CS8983

    public bool Foo { get; init; } = true;
}

struct StructWithFieldInitializer
{
    public StructWithFieldInitializer() { } // Compliant
    public int aField = 42;
}

struct StructWithPropertyInitializer
{
    public StructWithPropertyInitializer(int someParam) { } // Compliant
    public int AProperty { get; } = 42;
}

record struct RecordStructWithFieldInitializer
{
    public RecordStructWithFieldInitializer() { } // Compliant
    public int aField = 42;
}

record struct RecordStructWithPropertyInitializer
{
    public RecordStructWithPropertyInitializer() { } // Compliant
    public int AProperty { get; } = 42;
}

partial struct PartialStructWithPropertyInitializer
{
    public PartialStructWithPropertyInitializer() { } // Compliant
}

partial struct PartialStructWithPropertyInitializer
{
    public int AProperty { get; } = 42;
}
