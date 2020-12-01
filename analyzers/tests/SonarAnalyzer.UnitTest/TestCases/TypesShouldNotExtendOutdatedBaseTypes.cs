using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Tests.Diagnostics
{
    class Foo1 : System.ApplicationException { } // Noncompliant {{Refactor this type not to derive from an outdated type 'System.ApplicationException'.}}
//        ^^^^

    class Foo2 : XmlDocument { } // Noncompliant {{Refactor this type not to derive from an outdated type 'System.Xml.XmlDocument'.}}

    class Foo2_Explicit : System.Xml.XmlDocument { } // Noncompliant

    class Foo3 : System.Collections.CollectionBase { } // Noncompliant

    class Foo4 : System.Collections.DictionaryBase { } // Noncompliant

    class Foo5 : System.Collections.Queue { } // Noncompliant

    class Foo6 : System.Collections.ReadOnlyCollectionBase { } // Noncompliant

    class Foo7 : System.Collections.SortedList { } // Noncompliant

    class Foo8 : System.Collections.Stack { } // Noncompliant

    class Foo9 : Stack { } // Noncompliant

    class Foo10 : System.Collections.ICollection
    {
        public int Count { get { throw new NotImplementedException(); } }

        public bool IsSynchronized { get { throw new NotImplementedException(); } }

        public object SyncRoot { get { throw new NotImplementedException(); } }

        public void CopyTo(Array array, int index) { throw new NotImplementedException(); }

        public IEnumerator GetEnumerator() { throw new NotImplementedException(); }
    }

    class Foo11 : Dictionary<int, object> { }

    class Foo12 { }

    class FooBase : XmlDocument { } // Noncompliant

    class FooDerived : FooBase { } // Compliant - doesn't directly implement type

    class Foo13 : InvalidType{ } // Error [CS0246] - unknown type

    class : System.Collections.Stack { } // Error [CS1001] - missing identifier
}



