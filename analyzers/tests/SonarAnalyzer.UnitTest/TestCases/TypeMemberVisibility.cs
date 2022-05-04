using System;

namespace Tests.Diagnostics
{
    internal class Noncompliant // Noncompliant {{Types should not have members with visibility set higher than the type's visibility}}
    {
        public static decimal A = 3.14m;
//      ^^^^^^ Secondary
        protected internal static decimal B = 1m; // Improvement idea: check InternalsVisibleTo attributes.
        internal static decimal C = 1m;
        protected static decimal D = 1m;
        private static decimal F = 1m;

        public int PropertyA { get; }
//      ^^^^^^ Secondary
        protected internal int PropertyB { get; }
        internal int PropertyC { get; }
        protected int PropertyD { get; }
        private int PropertyF { get; }

        public int GetA() => 1;
//      ^^^^^^ Secondary
        protected internal int GetB() => 1;
        internal int GetC() => 1;
        protected int GetD() => 1;
        private int GetF() => 1;

        public event EventHandler ClickA;
//      ^^^^^^ Secondary
        protected internal event EventHandler ClickB;
        internal event EventHandler ClickC;
        protected event EventHandler ClickD;
        private event EventHandler ClickF;

        public delegate void DelegateA(string str);
//      ^^^^^^ Secondary
        protected internal delegate void DelegateB(string str);
        internal delegate void DelegateC(string str);
        protected delegate void DelegateD(string str);
        private delegate void DelegateF(string str);

        public Noncompliant(int a) {}
//      ^^^^^^ Secondary
        protected internal Noncompliant(int a, int b) {}
        internal Noncompliant(int a, int b, int c) {}
        protected Noncompliant(int a, int b, int c, int d) {}
        private Noncompliant(int a, int b, int c, int d, int e, int f) {}

        public int this[int index] => 1;
//      ^^^^^^ Secondary
        protected internal int this[string index] => 1;

        public struct NestedStructA
//      ^^^^^^ Secondary
        {
            public bool FlipCoin() => false;
//          ^^^^^^ Secondary
        }

        protected internal struct NestedStructB
        {
            public bool FlipCoin() => false;
//          ^^^^^^ Secondary
        }

        internal struct NestedStructC
        {
            public bool FlipCoin() => false;
//          ^^^^^^ Secondary
        }

        protected struct NestedStructD
        {
            public bool FlipCoin() => false;
//          ^^^^^^ Secondary
        }

        public enum Enum
//      ^^^^^^ Secondary
        {
            A
        }

        public static implicit operator byte(Noncompliant d) => 1;                   // Compliant: user defined operators need to be public
        public static explicit operator Noncompliant(byte b) => new Noncompliant(b); // Compliant: user defined operators need to be public
        public static Noncompliant operator +(Noncompliant b) => null;               // Compliant: user defined operators need to be public

        private class NestedPrivateClass
        {
            public int PublicProperty { get; }
//          ^^^^^^ Secondary
            protected internal int ProtectedInternalProperty { get; } // Compliant: can be used in `InternalsVisibleTo` assemblies
            internal int InternalProperty { get; } // Compliant: can be used in `InternalsVisibleTo` assemblies
            protected int ProtectedProperty { get; } // Compliant: can be used in derived type
            private int PrivateProperty { get; }
        }

        private class DerivedNestedPrivateClass : NestedPrivateClass
        {
            internal DerivedNestedPrivateClass()
            {
                var d = ProtectedProperty; // Protected properties can be accessed even if the base class is private.
            }
        }

        protected class NestedProtectedClass
        {
            public int PropertyA { get; }
//          ^^^^^^ Secondary
            internal int PropertyB { get; }
            protected internal int PropertyC { get; }
            protected int PropertyD { get; }
            private int PropertyF { get; }
        }

        private void Method()
        {
            var nestedPrivate = new NestedPrivateClass();
            var x = nestedPrivate.InternalProperty; // internal properties can be used from nested private classes
        }
    }

    internal struct Struct // Noncompliant
    {
        public class MyClass { }
//      ^^^^^^ Secondary

        public struct MyStruct { }
//      ^^^^^^ Secondary
        public enum MyEnum { }
//      ^^^^^^ Secondary
    }

    public class Compliant // Class visibility upgrade makes members compliant
    {
        public static decimal A = 3.14m;
        protected internal static decimal B = 1m;
        internal static decimal C = 1m;

        public int PropertyA { get; }
        protected internal int PropertyB { get; }
        internal int PropertyC { get; }

        public int GetA() => 1;
        protected internal int GetB() => 1;
        internal int GetC() => 1;

        public event EventHandler ClickA;
        public delegate void DelegateA(string str);
        public int this[int index] => 1;

        public Compliant(int a) {}
        protected internal Compliant(int a, int b) {}
        internal Compliant(int a, int b, int c) {}

        public struct NestedStructA
        {
            public bool FlipCoin() => false;
        }

        protected internal struct NestedStructB
        {
            internal bool FlipCoin() => false; // struct cannot have protected members
        }

        internal struct NestedStructC
        {
            internal bool FlipCoin() => false;
        }

        public class MyClass { }

        public struct MyStruct { }

        public enum MyEnum { }
    }

    internal sealed class WithMethodOverride
    {
        public override string ToString() => string.Empty;  // Compliant - the overriden method is public
    }

    public interface IContract
    {
        void Do();
    }

    internal class SomeClass: IContract
    {
        public void Do()
        {
            throw new NotImplementedException();
        }
    }

    internal class SomeClass2: IContract // Noncompliant
    {
        public void Do()
        {
            throw new NotImplementedException();
        }

        public void DoSomethingElse() { }
//      ^^^^^^ Secondary
    }

    public class Base
    {
        public Base() { }

        public Base(string x) { }

        public Base(string x, string y) { }
    }

    internal class Derived : Base // Noncompliant
    {
        public Derived() { }
//      ^^^^^^ Secondary

        public Derived(string x) : base(x) { } // The constructor visibility can be different from the base
//      ^^^^^^ Secondary
        internal Derived(string x, string y) : base(x, y) { }
    }
}
