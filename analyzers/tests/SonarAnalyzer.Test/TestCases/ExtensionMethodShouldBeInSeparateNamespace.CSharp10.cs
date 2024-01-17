namespace X;

public record class Record { }
public record struct RecordStruct { }

public static class GlobalExtensions
{
    public static void Bar(this Record r) { } // Noncompliant
    public static void Bar(this RecordStruct r) { } // Compliant
}
