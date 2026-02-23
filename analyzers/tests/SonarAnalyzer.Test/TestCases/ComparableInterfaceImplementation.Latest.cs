using System;
using System.Collections.Generic;

namespace Tests.Diagnostics.RecordTests
{
    public record RecordMissingAll : IComparable // Noncompliant {{When implementing IComparable, you should also override <, <=, >, and >=.}}
//                ^^^^^^^^^^^^^^^^
    {
        public int CompareTo(object obj) => 0;
    }

    public record RecordMissingOperators : IComparable<RecordMissingOperators> // Noncompliant {{When implementing IComparable<T>, you should also override <, <=, >, and >=.}}
//                ^^^^^^^^^^^^^^^^^^^^^^
    {
        public int CompareTo(RecordMissingOperators other) => 0;
    }

    public record RecordMissingLessThan : IComparable // Noncompliant {{When implementing IComparable, you should also override < and >.}}
//                ^^^^^^^^^^^^^^^^^^^^^
    {
        public int CompareTo(object obj) => 0;

        public static bool operator >=(RecordMissingLessThan left, RecordMissingLessThan right) => true;
        public static bool operator <=(RecordMissingLessThan left, RecordMissingLessThan right) => true;
    }

    public record RecordCompliant : IComparable // Compliant record auto-generates ==, !=, Equals
    {
        public int CompareTo(object obj) => 0;

        public static bool operator <(RecordCompliant left, RecordCompliant right) => true;
        public static bool operator >(RecordCompliant left, RecordCompliant right) => true;
        public static bool operator <=(RecordCompliant left, RecordCompliant right) => true;
        public static bool operator >=(RecordCompliant left, RecordCompliant right) => true;
    }

    public record DerivedRecordCompliant : RecordCompliant, IComparable // Compliant
    {
    }
}

namespace Tests.Diagnostics.RecordStructTests
{
    public record struct RecordStructMissingAll : IComparable // Noncompliant {{When implementing IComparable, you should also override <, <=, >, and >=.}}
//                       ^^^^^^^^^^^^^^^^^^^^^^
    {
        public int CompareTo(object obj) => 0;
    }

    public record struct RecordStructMissingOperators : IComparable<RecordStructMissingOperators> // Noncompliant {{When implementing IComparable<T>, you should also override <, <=, >, and >=.}}
//                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public int CompareTo(RecordStructMissingOperators other) => 0;
    }

    public record struct RecordStructMissingLessThan : IComparable // Noncompliant {{When implementing IComparable, you should also override < and >.}}
//                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public int CompareTo(object obj) => 0;

        public static bool operator >=(RecordStructMissingLessThan left, RecordStructMissingLessThan right) => true;
        public static bool operator <=(RecordStructMissingLessThan left, RecordStructMissingLessThan right) => true;
    }

    public record struct RecordStructCompliant : IComparable // Compliant record auto-generates ==, !=, Equals
    {
        public int CompareTo(object obj) => 0;

        public static bool operator <(RecordStructCompliant left, RecordStructCompliant right) => true;
        public static bool operator >(RecordStructCompliant left, RecordStructCompliant right) => true;
        public static bool operator <=(RecordStructCompliant left, RecordStructCompliant right) => true;
        public static bool operator >=(RecordStructCompliant left, RecordStructCompliant right) => true;
    }
}

namespace Tests.Diagnostics.PrivateRecordTests
{
    public class ContainerWithPrivateRecords
    {
        private record PrivateRecordComparable : IComparable // Compliant - private nested
        {
            public int CompareTo(object obj) => 0;
        }

        private record struct PrivateRecordStructComparable : IComparable // Compliant - private nested
        {
            public int CompareTo(object obj) => 0;
        }

        private record PrivateGenericRecordComparable : IComparable<PrivateGenericRecordComparable> // Compliant - private nested
        {
            public int CompareTo(PrivateGenericRecordComparable other) => 0;
        }

        private record struct PrivateGenericRecordStructComparable : IComparable<PrivateGenericRecordStructComparable> // Compliant - private nested
        {
            public int CompareTo(PrivateGenericRecordStructComparable other) => 0;
        }
    }

    internal record InternalRecordComparable : IComparable<InternalRecordComparable> // Noncompliant {{When implementing IComparable<T>, you should also override <, <=, >, and >=.}}
//                  ^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public int CompareTo(InternalRecordComparable other) => 0;
    }

    internal record struct InternalRecordStructComparable : IComparable<InternalRecordStructComparable> // Noncompliant {{When implementing IComparable<T>, you should also override <, <=, >, and >=.}}
//                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public int CompareTo(InternalRecordStructComparable other) => 0;
    }
}
