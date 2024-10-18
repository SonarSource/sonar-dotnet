using System;

record Foo
{
    public int this[Class index] { get { return 0; } }
//                  ^^^^^ Noncompliant {{Use string, integral, index or range type here, or refactor this indexer into a method.}}

    public int this[Record index] { get { return 0; } }
//                  ^^^^^^ Noncompliant {{Use string, integral, index or range type here, or refactor this indexer into a method.}}

    public int this[PositionalRecord index] { get { return 0; } } // Noncompliant

    public int this[Range range] { get { return 0; } }

    public int this[Index index] { get { return 0; } }

    public int this[int index] { get { return 0; } }

    public int this[string index] { get { return 0; } }

    public int this[nint index] { get { return 0; } } // Noncompliant Represented by the underlying type of System.IntPtr

    public int this[nuint index] { get { return 0; } } // Noncompliant Represented by the underlying type of System.UIntPtr
}

class Class { }
record Record { }
record PositionalRecord(int SomeProperty) { }


namespace CSharp13
{

    partial class PartialIndexerClass
    {
        public partial int this[Class index] { get; }
        //                      ^^^^^ Noncompliant {{Use string, integral, index or range type here, or refactor this indexer into a method.}}

        public partial int this[Record index] { get; }
        //                      ^^^^^^ Noncompliant {{Use string, integral, index or range type here, or refactor this indexer into a method.}}

        public partial int this[PositionalRecord index] { get; } // Noncompliant

        public partial int this[Range range] { get; }

        public partial int this[Index index] { get; }

        public partial int this[int index] { get; }

        public partial int this[string index] { get; }

        public partial int this[nint index] { get; } // Noncompliant Represented by the underlying type of System.IntPtr

        public partial int this[nuint index] { get; } // Noncompliant Represented by the underlying type of System.UIntPtr
    }
}
