using System;

namespace CSharp9
{
    internal record Noncompliant // Noncompliant {{Types should not have members with visibility set higher than the type's visibility}}
    {
        public static decimal A = 3.14m;
//      ^^^^^^ Secondary
        private protected decimal E = 1m;

        public int PropertyA { get; }
//      ^^^^^^ Secondary
        private protected int PropertyE { get; }

        public int GetA() => 1;
//      ^^^^^^ Secondary
        private protected int GetE() => 1;

        public event EventHandler ClickA;
//      ^^^^^^ Secondary
        private protected event EventHandler ClickE;

        public delegate void DelegateA(string str);
//      ^^^^^^ Secondary
        private protected delegate void DelegateE(string str);

        public Noncompliant(int a) {}
//      ^^^^^^ Secondary
        private protected Noncompliant(int a, int b, int c, int d, int e) {}

        public int this[int index] => 1;
//      ^^^^^^ Secondary
        protected internal int this[string index] => 1;

        public struct NestedStructA { }
//      ^^^^^^ Secondary

        private protected struct NestedStructE  { }

        public static implicit operator byte(Noncompliant d) => 1; // Compliant: user defined operators need to be public
        public static explicit operator Noncompliant(byte b) => new Noncompliant(b); // Compliant: user defined operators need to be public
    }

    internal record NoncompliantPositionalRecord(string Property) // Noncompliant {{Types should not have members with visibility set higher than the type's visibility}}
    {
        public static decimal A = 3.14m;
//      ^^^^^^ Secondary
    }

    internal class Class // Noncompliant
    {
        public record MyRecord { }
//      ^^^^^^ Secondary
    }

    internal record Record // Noncompliant
    {
        public record MyRecord { }
//      ^^^^^^ Secondary
    }

    internal struct Struct // Noncompliant
    {
        public record MyRecord { }
//      ^^^^^^ Secondary
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
            internal bool FlipCoin() => false;
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

namespace CSharp10
{
    internal record struct Noncompliant // Noncompliant {{Types should not have members with visibility set higher than the type's visibility}}
    //                     ^^^^^^^^^^^^
    {
        public Noncompliant() { } // Secondary

        static public decimal A = 3.14m; // Secondary
        //     ^^^^^^
        private decimal E = 1m;

        public int PropertyA { get; } = 0; // Secondary
        private int PropertyE { get; } = 0;

        public int GetA() => 1; // Secondary
        private int GetE() => 1;
    }

    internal record struct NoncompliantPositionalRecord(string Property) // Noncompliant
    {
        public static decimal A = 3.14m; // Secondary
    }
}


namespace CSharp11
{
    public interface IContract
    {
        static abstract void Do();
    }

    internal class SomeClass : IContract
    {
        public static void Do()
        {
            throw new NotImplementedException(); // Compliant because comes from the interface
        }
    }

    file class LocalFileClass // file-scoped types have no access modifiers. We don't need to check their members.
    {
        public void Foo() { }
    }
}

namespace CSharp13
{
    internal partial class PartialPropertyClass //Noncompliant
    {
        public partial int PropertyA { get; } // Secondary
        private partial int PropertyE { get; }
    }

    internal partial class OtherPartialPropertyClass //Noncompliant
    {
        public partial int PropertyA { get; } // Secondary
        private partial int PropertyE { get; }

        public partial void VoidMethod() { }   // Secondary
        partial void AnotherVoidMethod();

        public partial void DoSomething(); // Secondary
        private partial void DoSomethingElse();
    }
}
