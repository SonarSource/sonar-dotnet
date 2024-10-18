using System;

namespace CSharp13
{
    partial class PartialIndexerClass
    {
        public partial int this[Class index] { get { return 0; } }
        //                      ^^^^^ Noncompliant {{Use string, integral, index or range type here, or refactor this indexer into a method.}}

        public partial int this[Record index] { get { return 0; } }
        //                      ^^^^^^ Noncompliant {{Use string, integral, index or range type here, or refactor this indexer into a method.}}

        public partial int this[PositionalRecord index] { get { return 0; } } // Noncompliant

        public partial int this[Range range] { get { return 0; } }

        public partial int this[Index index] { get { return 0; } }

        public partial int this[int index] { get { return 0; } }

        public partial int this[string index] { get { return 0; } }

        public partial int this[nint index] { get { return 0; } } // Noncompliant Represented by the underlying type of System.IntPtr

        public partial int this[nuint index] { get { return 0; } } // Noncompliant Represented by the underlying type of System.UIntPtr
    }
}
