using System;
using System.Diagnostics;

[DebuggerDisplay("{RecordProperty}")]
public record SomeRecord(int RecordProperty)
{
    [DebuggerDisplay("{RecordProperty}")] public record NestedRecord1(int NestedRecordProperty);       // Noncompliant
    [DebuggerDisplay("{NestedRecordProperty}")] public record NestedRecord2(int NestedRecordProperty); // Compliant
}
