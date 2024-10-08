using System;
using System.Runtime.InteropServices;

public record Compliant
{
    public int field;
}

[StructLayout(LayoutKind.Sequential)]
public record InteropMethodArgument
{
    public uint number; // Compliant, we don't raise on members of classes with StructLayout attribute
}

public record Record
{
    private int field; // Noncompliant {{Remove unassigned field 'field', or set its value.}}
    private int field2;
    private static int field3; // Noncompliant
    private static int field4;
    private readonly int field5; // Noncompliant
    private int field6; // Compliant - value is set in nested class ctor

    private int Property { get; }  // Noncompliant
    private int Property2 { get; }

    public Record()
    {
        field2 = field;
        field4 = field3;
        field2 = field5;
        Property2 = Property;
    }

    private record NestedRecord
    {
        NestedRecord(Record r)
        {
            r.field6 = 5;
        }
    }
}

public record PositionalRecord(string Value)
{
    private int field; // Noncompliant
    private int field2;
    public PositionalRecord() : this("")
    {
        field2 = field;
    }
}

public partial class PartialProperties
{
    public partial int Prop1 => 42;
}
