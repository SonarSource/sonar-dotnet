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
    private int field; // Compliant - FN
    private int field2;
    private static int field3; // Compliant - FN
    private static int field4;
    private readonly int field5; // Compliant - FN
    private int field6; // Compliant - value is set in nested class ctor

    private int Property { get; }  // Compliant - FN
    private int Property2 { get; }

    public Record()
    {
        field2 = 1;
        field4 = 1;
        Property2 = 1;
    }

    private record NestedRecord
    {
        NestedRecord(Record r)
        {
            r.field6 = 5;
        }
    }
}
