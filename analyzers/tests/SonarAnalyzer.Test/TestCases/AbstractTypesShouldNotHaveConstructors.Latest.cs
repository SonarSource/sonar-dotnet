using System;
using System.Collections.Generic;

namespace CSharp12
{
    abstract class PrimaryConstructor(int i) // Compliant
    {
        public int I { get; set; } = i;
    }
}

namespace Records
{
    abstract record AbstractRecordOne
    {
        public string X { get; }

        public AbstractRecordOne(string x) => (X) = (x); // Noncompliant
    }

    record RecordOne : AbstractRecordOne
    {
        public RecordOne(string x) : base(x) { } // Compliant
    }

    abstract record AbstractRecordTwo(string Y);

    record RecordTwo(string Z) : AbstractRecordTwo(Z);

    public abstract record Person(string Name, string Surname)
    {
        public Person(string name) : this(name, "") // Noncompliant
        {
        }
    }

    public struct MyStruct
    {
        public MyStruct(string s)
        {
        }
    }
}

namespace CSharp14
{
    public abstract partial class PartialConstructor
    {
        public partial PartialConstructor();
    }

    public abstract partial class NonPartialConstructor
    {
        public NonPartialConstructor() { }  // Noncompliant
    }
}

namespace CSharp15
{
    public closed class ClosedWithPublicCtor
    {
        public ClosedWithPublicCtor() { } // Noncompliant {{Change the visibility of this constructor to 'protected'.}} - NET-4874
    }

    public closed class ClosedWithInternalCtor
    {
        internal ClosedWithInternalCtor() { } // Noncompliant {{Change the visibility of this constructor to 'private protected'.}}
    }

    public closed class ClosedWithNonPublicCtors
    {
        protected ClosedWithNonPublicCtors() { }               // Compliant
        private protected ClosedWithNonPublicCtors(int i) { }  // Compliant
        private ClosedWithNonPublicCtors(string s) { }         // Compliant
    }

    public closed record ClosedRecordWithPublicCtor
    {
        public ClosedRecordWithPublicCtor(int i) { } // Noncompliant
    }

    public closed record ClosedRecordWithProtectedCtor
    {
        protected ClosedRecordWithProtectedCtor(int i) { } // Compliant
    }

    public closed partial class ClosedPartial { }

    public partial class ClosedPartial
    {
        public ClosedPartial() { } // FN NET-5286
    }
}
