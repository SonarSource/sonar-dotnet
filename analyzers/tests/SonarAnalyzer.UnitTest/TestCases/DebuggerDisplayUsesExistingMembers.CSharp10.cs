using System;
using System.Diagnostics;

[DebuggerDisplay("{RecordProperty}")]
public record SomeRecord(int RecordProperty)
{
    [DebuggerDisplay("{RecordProperty}")] public record struct RecordStruct1(int RecordStructProperty);       // Noncompliant
    [DebuggerDisplay("{RecordStructProperty}")] public record struct RecordStruct2(int RecordStructProperty); // Compliant
}

[DebuggerDisplay("{RecordProperty1} bla bla {RecordProperty2}")]
public record struct SomeRecordStruct(int RecordProperty1, string RecordProperty2)
{
    [DebuggerDisplay("{RecordProperty}")]            // Noncompliant
    public class NestedClass1
    {
        [DebuggerDisplay("{NestedClassProperty}")]   // Compliant
        public int NestedClassProperty => 1;
    }

    [DebuggerDisplay("{NestedClassProperty}")]       // Compliant
    public class NestedClass2
    {
        [DebuggerDisplay("{NestedClassProperty}")]   // Compliant
        public int NestedClassProperty => 1;
    }
}

public class SupportConstantInterpolatedStrings
{
    [DebuggerDisplay($"{{{nameof(SomeProperty)}}}")] // Compliant
    [DebuggerDisplay($"{{{nameof(SomeProperty)}}}")] // Compliant, FN: constant interpolated strings not supported
    public int SomeProperty => 1;
}
