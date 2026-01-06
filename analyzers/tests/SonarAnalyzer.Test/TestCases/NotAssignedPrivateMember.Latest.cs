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

public class FieldKeyWord
{
    public int Prop1 => field;
    private int Prop2 => field;                               // Compliant handled by S2292
    public int Prop3 { get => field; set => field = value; }
    private int Prop4 { get => field; set => field = value; } // Compliant handled by S2292
}

public class NullConditionalAssignment
{
    private int compliant;                      // Compliant https://sonarsource.atlassian.net/browse/NET-2748
    private int compliantNonNull;               // Compliant
    private Nesting NestedCompliant;            // Compliant
    private Nesting NestedCompliantNonNull;     // Compliant
    private Nesting DeeperNoncompliant;         // Noncompliant
    private Nesting DeeperNoncompliantNonNull;  // Noncompliant
    private Nesting OtherDeeperNoncompliant;    // Noncompliant
    private Nesting AnotherDeeperNoncompliant;  // Noncompliant

    public NullConditionalAssignment()
    {
        this?.compliant = 42;
        compliantNonNull = 42;

        this?.NestedCompliant = new Nesting();
        NestedCompliantNonNull = new Nesting();

        DeeperNoncompliantNonNull.Prop = new Nesting();
        this?.DeeperNoncompliant?.Prop = new Nesting();
        this?.OtherDeeperNoncompliant.Prop = new Nesting();
        AnotherDeeperNoncompliant?.Prop = new Nesting();
    }

    public class Nesting
    {
        public Nesting Prop { get; set; }
        public Nesting this[int index] { get { return new Nesting(); } set { } }
    }
}

public static class Extensions
{
    private static string NonCompliantInProp;   // Noncompliant
    private static string CompliantInMethod;

    extension(string s)
    {
        public int length => NonCompliantInProp.Length;
        public void SetLength()
        {
            CompliantInMethod = "42";
        }
    }
}
