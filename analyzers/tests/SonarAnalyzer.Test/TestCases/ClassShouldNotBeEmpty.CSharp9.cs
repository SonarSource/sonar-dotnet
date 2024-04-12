using System;

namespace Compliant
{
    record RecordWithParameters(int Parameter);

    record RecordWithProperty
    {
        int SomeProperty => 42;
    }

    record RecordWithField
    {
        int SomeField = 42;
    }
    record RecordWithMethod
    {
        void Method() { }
    }
    record RecordWithMethodOverride
    {
        public override string ToString() => "";
    }
    record RecordWithIndexer
    {
        int this[int index] => 42;
    }
    record RecordWithDelegate
    {
        delegate void MethodDelegate();
    }
    record RecordWithEvent
    {
        event EventHandler CustomEvent;
    }

    record NotEmptyGenericRecord<T>(T RecordMember);

    record NotEmptyGenericRecordWithContraint<T>(T RecordMember) where T : class;

    record EmptyRecordProvidingParameterToBase() : RecordWithParameters(42);
}

namespace Noncompliant
{
    record EmptyRecord();                                        // Noncompliant {{Remove this empty record, write its code or make it an "interface".}}
    //     ^^^^^^^^^^^
    record EmptyRecordWithEmptyBody() { };                       // Noncompliant

    record EmptyChildWithoutBrackets : EmptyRecord;              // Noncompliant

    record EmptyChildRecord() : EmptyRecord();                   // Noncompliant

    record EmptyGenericRecord<T>();                              // Noncompliant
    //     ^^^^^^^^^^^^^^^^^^
    record EmptyGenericRecordWithContraint<T>() where T : class; // Noncompliant

    record ConstructorIsPublicAlready : EmptyRecord { }          // Noncompliant

    record BaseRecordWithProtectedConstructor
    {
        protected BaseRecordWithProtectedConstructor() { }
    }

    record WidensConstructorVisibility : BaseRecordWithProtectedConstructor { } // Compliant
}

namespace Ignore
{
    partial record EmptyPartialRecord(); // Compliant - partial classes are ignored, so partial record classes are ignored as well
}

// https://github.com/SonarSource/sonar-dotnet/issues/7709
namespace Repro_7709
{
    interface IMarker { }
    record ImplementsMarker : IMarker { }
    record ImplementsEmptyRecordAndMarker : Noncompliant.EmptyRecord, IMarker { }
}
