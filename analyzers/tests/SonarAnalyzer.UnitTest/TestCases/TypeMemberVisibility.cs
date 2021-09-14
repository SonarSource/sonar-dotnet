using System;

namespace Tests.Diagnostics
{
    internal class Noncompliant
    {
        public static decimal A = 3.14m; // Noncompliant
        protected internal static decimal B = 1m; // Improvement idea: check InternalsVisibleTo attributes.
        internal static decimal C = 1m;
        protected static decimal D = 1m;
        private protected decimal E = 1m;
        private static decimal F = 1m;

        public int PropertyA { get; } // Noncompliant
        protected internal int PropertyB { get; }
        internal int PropertyC { get; }
        protected int PropertyD { get; }
        private protected int PropertyE { get; }
        private int PropertyF { get; }

        public int GetA() => 1; // Noncompliant
        protected internal int GetB() => 1;
        internal int GetC() => 1;
        protected int GetD() => 1;
        private protected int GetE() => 1;
        private int GetF() => 1;

        public event EventHandler ClickA; // Noncompliant
        protected internal event EventHandler ClickB;
        internal event EventHandler ClickC;
        protected event EventHandler ClickD;
        private protected event EventHandler ClickE;
        private event EventHandler ClickF;

        public delegate void DelegateA(string str);  // Noncompliant
        protected internal delegate void DelegateB(string str);
        internal delegate void DelegateC(string str);
        protected delegate void DelegateD(string str);
        private protected delegate void DelegateE(string str);
        private delegate void DelegateF(string str);

        public Noncompliant(int a) {} // Noncompliant
        protected internal Noncompliant(int a, int b) {}
        internal Noncompliant(int a, int b, int c) {}
        protected Noncompliant(int a, int b, int c, int d) {}
        private protected Noncompliant(int a, int b, int c, int d, int e) {}
        private Noncompliant(int a, int b, int c, int d, int e, int f) {}

        public int this[int index] => 1; // Noncompliant
        protected internal int this[string index] => 1;

        public struct NestedStructA // Noncompliant
        {
            public bool FlipCoin() => false; // Noncompliant
        }

        protected internal struct NestedStructB
        {
            public bool FlipCoin() => false; // Noncompliant
        }

        internal struct NestedStructC
        {
            public bool FlipCoin() => false; // Noncompliant
        }

        protected struct NestedStructD
        {
            public bool FlipCoin() => false; // Noncompliant
        }

        private protected struct NestedStructE
        {
            public bool FlipCoin() => false; // Noncompliant
        }

        public record NestedRecordA // Noncompliant
        {
            public bool FlipCoin() => false; // Noncompliant
        }

        public enum Enum // Noncompliant
        {
            A
        }

        public static implicit operator byte(Noncompliant d) => 1; // Compliant: user defined operators need to be public
        public static explicit operator Noncompliant(byte b) => new Noncompliant(b); // Compliant: user defined operators need to be public

        private class NestedPrivateClass
        {
            public int PublicProperty { get; } // Noncompliant: should be internal
            protected internal int ProtectedInternalProperty { get; } // Compliant: can be used in `InternalsVisibleTo` assemblies
            internal int InternalProperty { get; } // Compliant: can be used in `InternalsVisibleTo` assemblies
            protected int ProtectedProperty { get; } // Compliant: can be used in derived type
            private protected int PrivateProtectedProperty { get; } // Compliant: can be used in derived type
            private int PrivateProperty { get; }
        }

        private class DerivedNestedPrivateRecord : NestedPrivateClass
        {
            internal DerivedNestedPrivateRecord()
            {
                var d = ProtectedProperty; // Protected properties can be accessed even if the base class is private.
                var e = PrivateProtectedProperty; // Private protected properties can be accessed even if the base class is private.
            }
        }

        protected class NestedProtectedRecord
        {
            public int PropertyA { get; } // Noncompliant: can be internal
            internal int PropertyB { get; }
            protected internal int PropertyC { get; }
            protected int PropertyD { get; }
            private protected int PropertyE { get; }
            private int PropertyF { get; }
        }

        private void Method()
        {
            var nestedPrivate = new NestedPrivateClass();
            var x = nestedPrivate.InternalProperty; // internal properties can be used from nested private classes
        }
    }

    internal record Record
    {
        public class MyClass { } // Noncompliant

        public struct MyStruct { } // Noncompliant

        public enum MyEnum { } // Noncompliant

        public record MyRecord { } // Noncompliant
    }

    internal struct Struct
    {
        public class MyClass { } // Noncompliant

        public struct MyStruct { } // Noncompliant

        public enum MyEnum { } // Noncompliant

        public record MyRecord { } // Noncompliant
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

        public record MyRecord { }
    }
}
