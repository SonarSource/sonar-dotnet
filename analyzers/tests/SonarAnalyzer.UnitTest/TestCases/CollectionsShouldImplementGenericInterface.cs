using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.Diagnostics
{
    class TestClass_CollectionBase : CollectionBase { } // Noncompliant {{Refactor this collection to implement 'System.Collections.ObjectModel.Collection<T>'.}}
    //    ^^^^^^^^^^^^^^^^^^^^^^^^
    class TestClass_IList : IList { }                   // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.IList<T>'.}}
    class TestClass_IEnumerable : IEnumerable { }       // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.IEnumerable<T>'.}}
    class TestClass_ICollection : ICollection { }       // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.ICollection<T>'.}}

    class TestClass_TwoInterfaces : IEnumerable, ICollection { } // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.IEnumerable<T>'.}}
    // Noncompliant@-1 {{Refactor this collection to implement 'System.Collections.Generic.ICollection<T>'.}}

    class TestClass_NonGenericAndGenericCollection : IEnumerable, ICollection<string> { }
    class TestClass_GenericAndNonGeneric : ICollection<string>, IEnumerable { }
    class TestClass_NonGenericAndIList : IEnumerable, IList<string> { }
    class TestClass_NonGenericAndGeneric : IEnumerable, IEnumerable<string> { }
    class TestClass_GenericBaseAndNonGeneric : Collection<string>, IEnumerable { }
    class TestClass_WithTypeParameter_1<T> : IEnumerable, IList<T> { }
    class TestClass_WithTypeParameter_2<T> : IEnumerable, ICollection, ICollection<T> { }

    class TestClass_MultipleInterfaces : IEnumerable, IList, IEnumerable<string> { }
    class TestClass_NoInterfaces { }

    class TestClass_UnrelatedBaseClass : Exception { }
    class TestClass_UnrelatedInterface : IEqualityComparer { }
    class TestClass_WithCompilerError : IList, InvalidType { } // Noncompliant

    struct TestStruct_NongenericIList : IList { }              // Noncompliant
    struct TestStruct_NonGenericIEnumerable : IEnumerable { }  // Noncompliant

    class BaseClassEnumerable : IEnumerable<int> { }
    class Derived : BaseClassEnumerable, IEnumerable { } // Noncompliant FP: Interfaces in base classes are ignored.
}
