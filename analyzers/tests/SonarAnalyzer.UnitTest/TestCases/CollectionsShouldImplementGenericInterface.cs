using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.Diagnostics
{
    class TestClass_01 : CollectionBase { } // Noncompliant {{Refactor this collection to implement 'System.Collections.ObjectModel.Collection<T>'.}}
//        ^^^^^^^^^^^^
    class TestClass_02 : IList { } // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.IList<T>'.}}

    class TestClass_03 : IEnumerable { } // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.IEnumerable<T>'.}}

    class TestClass_04 : ICollection { } // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.ICollection<T>'.}}

    class TestClass_05 : IEnumerable, ICollection { } // Noncompliant {{Refactor this collection to implement 'System.Collections.Generic.IEnumerable<T>'.}}
    // Noncompliant@-1 {{Refactor this collection to implement 'System.Collections.Generic.ICollection<T>'.}}

    class TestClass_06 : IEnumerable, ICollection<string> { }
    class TestClass_07 : ICollection<string>, IEnumerable { }
    class TestClass_08 : IEnumerable, IList<string> { }
    class TestClass_09 : IEnumerable, IEnumerable<string> { }
    class TestClass_10 : Collection<string>, IEnumerable { }
    class TestClass_11<T> : IEnumerable, IList<T> { }

    class TestClass_12<T> : IEnumerable, ICollection, ICollection<T> { }

    class TestClass_13 : IEnumerable, IList, IEnumerable<string> { }

    class TestClass_14 { }

    class TestClass_15 : Exception { }

    class TestClass_16 : IEqualityComparer { }

    class TestClass_17 : IList, InvalidType { } // Noncompliant

    struct TestStruct_01 : IList { } // Noncompliant

    struct TestStruct_02 : IEnumerable { } // Noncompliant
}
