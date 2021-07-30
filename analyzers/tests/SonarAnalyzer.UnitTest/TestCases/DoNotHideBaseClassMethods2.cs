using System;
using System.Collections.Generic;

namespace AppendedNamespaceForConcurrencyTest.MyLibrary
{
    class Foo
    {
        public void SomePublicMethod(string s1, string s2) { }

        protected void SomeProtectedMethod(string s1, string s2) { }

        private void SomePrivateMethod(string s1, string s2) { }

        public void GenericMethod<T>(T s1, string s2) { }

        public void GenericMethod2<T>(IEnumerable<T> s1, string s2) { }
    }

    class Bar : Foo
    {
        public void SomePublicMethod(string s1, object s2) { } // Noncompliant {{Remove or rename that method because it hides 'AppendedNamespaceForConcurrencyTest.MyLibrary.Foo.SomePublicMethod(string, string)'.}}
//                  ^^^^^^^^^^^^^^^^
        protected void SomeProtectedMethod(string s1, object o2) { } // Noncompliant

        private void SomePrivateMethod(string s1, object o2) { }

        public void SomePublicMethod(string s1, string s2) { }

        public void GenericMethod<TType>(TType s1, object s2) { } // Noncompliant

        public void GenericMethod2<T>(IEnumerable<T> s1, object s2) { } // Noncompliant
    }

    class Bar2 : Foo
    {
    }

    class Bar3 : Bar2
    {
        public void SomePublicMethod(string s1, object o2) { } // Noncompliant
    }


    class MultipleOverloadsBase
    {
        public bool Method1(object obj) => true;

        public bool Method1(string obj) => true;
    }

    class MultipleOverloadsDerived : MultipleOverloadsBase
    {
        public bool Method1(object obj) => true;
    }
}

namespace AppendedNamespaceForConcurrencyTest.OtherNamespace
{
    class Class1
    {
        internal void SomeMethod(string s) { }
    }

    class Class2 : Class1
    {
        void SomeMethod(object s) { } // Noncompliant
    }
}

namespace AppendedNamespaceForConcurrencyTest.MyNamespace
{
    class Class3 : AppendedNamespaceForConcurrencyTest.OtherNamespace.Class1
    {
        void SomeMethod(object s) { }
    }
}
