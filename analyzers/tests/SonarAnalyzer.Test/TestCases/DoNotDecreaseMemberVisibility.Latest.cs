using System;
using System.Collections.Generic;

namespace MyLibrary
{
    public interface IMyInterface
    {
        public static virtual void MyMethod() { }
    }

    public interface IMyOtherInterface : IMyInterface
    {
        private static void MyMethod() { }
    }

    public class MyClass<T> where T : IMyOtherInterface
    {
        public MyClass(IMyOtherInterface other)
        {
            T.MyMethod(); // Compliant, the method from IMyInterface is called
        }
    }

    record A
    {
        public void Method_01(int count) { }

        public void Method_02(int count) { }

        public void Method_03(int count) { }

        protected void Method_04(int count) { }

        public void Method_05(int count, string foo, params object[] args) { }

        public void Method_06<T>(T count) { }

        public virtual void Method_07(int count) { }

        public virtual void Method_08(int count) { }

        public void Method_09(int count) { }

        // No method 10

        public void Method_11<T>(int count) { }

        public void Method_12<T, V>(T obj, V obj2) { }

        public void Method_13<T, V>(T obj, IEnumerable<V> obj2) { }

        public void Method_14(int count) { }

        public void Method_15(out int count) { count = 2; }

        public void Method_16(ref int count) { }

        public virtual void Method_17(int count) { }


        public int Property_01 { get; init; }

        public int Property_02 { get; init; }

        public int Property_03 { get; private set; }

        public int Property_04 { get; }

        public int Property_05 { get; init; }

        public int Property_06 { get; init; }

        public virtual int Property_07 { get; init; }

        public virtual int Property_08 { get; init; }

        public virtual int Property_09 { get; init; }

        public int Property_10 { get; init; }

        public int Property_11 { get; init; }
    }

    record B : A
    {
    }

    record C : B
    {
        private void Method_01(int count) { } // Noncompliant {{This member hides 'MyLibrary.A.Method_01(int)'. Make it non-private or seal the class.}}

        protected void Method_02(int count) { } // Noncompliant

        void Method_03(int count) { } // Noncompliant

        private void Method_04(int count) { } // Noncompliant

        private void Method_05(int something, string something2, params object[] someArgs) { } // Noncompliant

        private void Method_06<T>(T count) { } // Noncompliant

        // Error@+1 [CS0621]
        private virtual void Method_07(int count) { }  // Noncompliant

        // Error@+1 [CS0621,CS0507]
        private override void Method_08(int count) { }  // Noncompliant

        private new void Method_09(int count) { }

        private void Method_10(int count) { }

        private void Method_11<V>(int count) { } // Noncompliant

        private void Method_12<V, T>(T obj, V obj2) { } // Noncompliant

        private void Method_13<V, T>(T obj, IEnumerable<T> obj2) { } // Noncompliant

        private void Method_14(int count) { } // Noncompliant

        private void Method_15(int count) { }

        private void Method_16(out int count) { count = 2; }

        // Error@+1 [CS0621,CS0507]
        private override void Method_17(int count) { } // Noncompliant


        private int Property_01 { get; init; } // Noncompliant

        public int Property_02 { private get; init; } // Noncompliant

        private int Property_03 { get; init; } // Noncompliant

        public int Property_04 { get; private init; }

        public int Property_05 { get; } // Noncompliant

        private int Property_06 { get; } // Noncompliant

        int i;

        // Note this cannot be auto-property, as it is a compiler error.
        public override int Property_07 { get { return i; } }

        // Error@+1 [CS0507] - cannot change modifier
        public override int Property_08 { get { return i; } private init { i = value; } } // Noncompliant

        public override int Property_09 { get; } // Error [CS8080]

        private string Property_10 { get; init; } // Noncompliant, return type is irrelevant for method resolution

        public int Property_11 { get; protected init; } // Noncompliant
    }

    record Foo
    {
        public void Method_01(int count) { }
    }

    sealed record Bar : Foo
    {
        private void Method_01(int count) { }
    }
}

namespace PositionalRecords
{
    record FooBase(string X)
    {
        public void Method(int count) { }
    }

    record Foo(string X) : FooBase(X)
    {
        private void Method(int count) { } // Noncompliant {{This member hides 'PositionalRecords.FooBase.Method(int)'. Make it non-private or seal the class.}}
    }
}

namespace OtherNamespace
{
    public record Class1
    {
        internal void SomeMethod(string s) { }
    }

    public record Class2 : Class1
    {
        private void SomeMethod(string s) { } // Noncompliant
    }
}

namespace SomeNamespace
{
    public record Class3 : OtherNamespace.Class1
    {
        private void SomeMethod(string s) { }
    }
}

namespace Indexers
{
    public record BaseClass
    {
        public int this[int index]
        {
            get { return index; }
            set { }
        }
    }

    public record DescendantClass : BaseClass
    {
        public int this[string name] // Compliant, parameters are of different types
        {
            get { return name.Length; }
        }
    }
}

namespace PartialProperties
{
    public class BaseClass
    {
        public int Property_01 { get; }
        public int this[int index]
        {
            get { return index; }
        }
    }

    public partial class DescendantClass : BaseClass
    {
        private partial int Property_01 { get; } // Noncompliant
        //                  ^^^^^^^^^^^
        private partial int this[int index] { get; } // Noncompliant
        //                  ^^^^
    }

    public partial class DescendantClass : BaseClass
    {
        private partial int Property_01 { get { return 1; } } // Noncompliant
        //                  ^^^^^^^^^^^
        private partial int this[int index] { get { return index + 1; } } // Noncompliant
        //                  ^^^^
    }

    // https://sonarsource.atlassian.net/browse/NET-368
    public class AnotherClass : BaseClass
    {
        private int this[int index] // Noncompliant
        {
            get { return index + 1; }
        }
    }
}
