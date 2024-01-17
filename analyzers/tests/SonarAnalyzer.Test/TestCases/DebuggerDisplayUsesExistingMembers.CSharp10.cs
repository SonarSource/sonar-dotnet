using System;
using System.Diagnostics;

[DebuggerDisplay("{RecordProperty}")]
public record SomeRecord(int RecordProperty)
{
    [DebuggerDisplay("{RecordProperty}")] public record struct RecordStruct1(int RecordStructProperty);       // Noncompliant
    [DebuggerDisplay("{RecordStructProperty}")] public record struct RecordStruct2(int RecordStructProperty); // Compliant, RecordStructProperty is a property

    [DebuggerDisplay("{RecordProperty}")] public record NestedRecord1(int NestedRecordProperty);       // Noncompliant
    [DebuggerDisplay("{NestedRecordProperty}")] public record NestedRecord2(int NestedRecordProperty); // Compliant, NestedRecordProperty is a property
}

[DebuggerDisplay("{RecordProperty1} bla bla {RecordProperty2}")]
public record struct SomeRecordStruct(int RecordProperty1, string RecordProperty2)
{
    [DebuggerDisplay("{RecordProperty}")]            // Noncompliant
    public class NestedClass1
    {
        [DebuggerDisplay("{NestedClassProperty}")]
        public int NestedClassProperty => 1;
    }

    [DebuggerDisplay("{NestedClassProperty}")]
    public class NestedClass2
    {
        [DebuggerDisplay("{NestedClassProperty}")]
        public int NestedClassProperty => 1;
    }
}

public class ConstantInterpolatedStrings
{
    [DebuggerDisplay($"{{{nameof(SomeProperty)}}}")]
    [DebuggerDisplay($"{{{nameof(NotAProperty)}}}")] // FN: constant interpolated strings not supported
    public int SomeProperty => 1;

    public class NotAProperty { }
}

public interface DefaultInterfaceImplementations
{
    [DebuggerDisplay("{OtherProperty}")]
    [DebuggerDisplay("{OtherPropertyImplemented}")]
    [DebuggerDisplay("{Nonexistent}")]               // Noncompliant
    int WithNonexistentProperty => 1;

    string OtherProperty { get; }
    string OtherPropertyImplemented => "Something";
}
