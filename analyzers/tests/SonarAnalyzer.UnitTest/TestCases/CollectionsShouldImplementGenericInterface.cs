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
    class TestClass_07 : IEnumerable, IList<string> { }
    class TestClass_08 : IEnumerable, IEnumerable<string> { }
    class TestClass_09 : Collection<string>, IEnumerable { }
    class TestClass_10<T> : IEnumerable, IList<T> { }

    class TestClass_11<T> : IEnumerable, ICollection, ICollection<T> { }

    class TestClass_12 : IEnumerable, IList, IEnumerable<string> { }

    class TestClass_13 { }

    class TestClass_14 : Exception { }

    class TestClass_15 : IEqualityComparer { }

    class TestClass_16 : IList, InvalidType { } // Noncompliant
}
