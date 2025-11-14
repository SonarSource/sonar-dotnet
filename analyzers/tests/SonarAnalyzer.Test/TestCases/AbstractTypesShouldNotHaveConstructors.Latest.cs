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
