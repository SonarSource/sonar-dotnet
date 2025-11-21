using System.Collections.Generic;

namespace MyLibrary
{
    public record Base
    {
        public void SomePublicMethod(string s1, string s2) { }

        public void AnotherPublicMethod(string s1, string s2) { }

        protected void SomeProtectedMethod(string s1, string s2) { }

        protected void SomeProtectedMethod(string s1, int s2) { }

        private void SomePrivateMethod(string s1, string s2) { }

        public void GenericMethod<T>(T s1, string s2) { }

        public void GenericMethod2<T>(IEnumerable<T> s1, string s2) { }
    }

    record Bar : Base
    {
        public void SomePublicMethod(string s1, object s2) { } // Noncompliant {{Remove or rename that method because it hides 'MyLibrary.Base.SomePublicMethod(string, string)'.}}
        protected void SomeProtectedMethod(string s1, object o2) { } // Noncompliant

        private void SomePrivateMethod(string s1, object o2) { }

        public void SomePublicMethod(string s1, string s2) { }

        public void GenericMethod<TType>(TType s1, object s2) { } // Noncompliant

        public void GenericMethod2<T>(IEnumerable<T> s1, object s2) { } // Noncompliant
    }

    record Bar2 : Base
    {
    }

    record Bar3 : Bar2
    {
        public void SomePublicMethod(string s1, object o2) { } // Noncompliant
    }


    record MultipleOverloadsBase
    {
        public bool Method1(object obj) => true;

        public bool Method1(string obj) => true;
    }

    record MultipleOverloadsDerived : MultipleOverloadsBase
    {
        public bool Method1(object obj) => true;
    }

    record PrimaryConstructorBase(string X)
    {
        public void Method(string s1) { }
    }

    record Derived(string X) : PrimaryConstructorBase(X)
    {
        public void Method(object s1) { } // Noncompliant {{Remove or rename that method because it hides 'MyLibrary.PrimaryConstructorBase.Method(string)'.}}
    }

    interface IMyInterface
    {
        static virtual string SomeMethod(string s1, string s2) => $"{s1}{s2}";
    }

    class IMyClass : IMyInterface
    {
        static string SomeMethod(string s1, object s2) => "With object"; // Compliant
    }
}

namespace OtherNamespace
{
    record Class1
    {
        internal void SomeMethod(string s) { }
    }

    record Class2 : Class1
    {
        void SomeMethod(object s) { } // Noncompliant
    }
}

namespace MyNamespace
{
    record Class3 : OtherNamespace.Class1
    {
        void SomeMethod(object s) { }
    }
}

public static class Extensions
{
    public static void SomePublicMethod(MyLibrary.Base sender, string s1, object s2) { } // Compliant, it's an extension method

    extension(MyLibrary.Base sender)
    {
        public void AnotherPublicMethod(string s1, string s2) { } // Compliant, it's an extension method
    }
}
