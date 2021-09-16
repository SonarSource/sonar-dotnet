using System;

namespace Tests.Diagnostics
{
    internal record Noncompliant
    {
        public static decimal A = 3.14m; // Noncompliant {{Types should not have members with visibility set higher than the type's visibility}}
//      ^^^^^^
        private protected decimal E = 1m;

        public int PropertyA { get; } // Noncompliant
//      ^^^^^^
        private protected int PropertyE { get; }

        public int GetA() => 1; // Noncompliant
        private protected int GetE() => 1;

        public event EventHandler ClickA; // Noncompliant
        private protected event EventHandler ClickE;

        public delegate void DelegateA(string str);  // Noncompliant
        private protected delegate void DelegateE(string str);

        public Noncompliant(int a) {} // Noncompliant
        private protected Noncompliant(int a, int b, int c, int d, int e) {}

        public int this[int index] => 1; // Noncompliant
        protected internal int this[string index] => 1;

        public struct NestedStructA { } // Noncompliant

        private protected struct NestedStructE  { }

        public static implicit operator byte(Noncompliant d) => 1; // Compliant: user defined operators need to be public
        public static explicit operator Noncompliant(byte b) => new Noncompliant(b); // Compliant: user defined operators need to be public
    }

    internal class Class
    {
        public record MyRecord { } // Noncompliant
    }

    internal record Record
    {
        public record MyRecord { } // Noncompliant
    }

    internal struct Struct
    {
        public record MyRecord { } // Noncompliant
    }

    public record Compliant // Class visibility upgrade makes members compliant
    {
        public static decimal A = 3.14m;
        internal static decimal C = 1m;

        public int PropertyA { get; }
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
