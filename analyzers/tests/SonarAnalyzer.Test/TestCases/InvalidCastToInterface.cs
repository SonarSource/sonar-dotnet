using System;
using System.Collections.Generic;


namespace Tests.Diagnostics
{
    public interface IMyInterface
    { /* ... */ }

    public class Implementer : IMyInterface { }

    public interface IMyInterface2
    { /* ... */ }

    public interface IMyInterface4 : IMyInterface
    { /* ... */ }

    public interface IMyInterface3 : IMyInterface
    { /* ... */ }

    public class MyClass1
    { /* ... */ }
    public class MyClass2
    { /* ... */ }

    public class MyClass3 : MyClass2, IMyInterface
    { /* ... */ }

    public class MyClass4
    { /* ... */ }

    public class InvalidCastToInterface
    {
        public class Nested : MyClass4, IDisposable
        {
            public void Dispose() { }
        }

        static void Main()
        {
            var myclass1 = new MyClass1();
            var x = (IMyInterface)myclass1; // Noncompliant {{Review this cast; in this project there's no type that extends 'MyClass1' and implements 'IMyInterface'.}}
//                   ^^^^^^^^^^^^
            x = myclass1 as IMyInterface;
            bool b = myclass1 is IMyInterface;

            var arr = new MyClass1[10];
            var arr2 = (IMyInterface[])arr;

            var myclass2 = new MyClass2();
            var y = (IMyInterface)myclass2;

            IMyInterface i = new MyClass3();
            var c = (IMyInterface2)i; // Compliant
            IMyInterface4 ii = null;
            var d = (IMyInterface2)i; // Compliant
            var e = (IMyInterface3)i;

            var o = (object)true;
            e = (IMyInterface3)o;

            var coll = (IEnumerable<int>)new List<int>();

            var z = (IDisposable)new MyClass4();

            var w = (IDisposable)(new Node());
        }
    }

    public class DerivedNode : MiddleNode, IDisposable
    {
        public void Dispose() { }
    }
    public class MiddleNode : Node
    {

    }
    public class Node
    { }

    public class MyClass
    {
        public double? D { get; set; } = 1.001;
    }

    interface IFoo { }
    interface IBar { }

    class Foo : IFoo { }
    class Bar : IBar { public Bar(string foo) { } }
    class FooBar : IFoo, IBar { }
    sealed class FinalBar : IBar { }

    class Other
    {
        public void Method<T>(T generic) where T : new()
        {
            IFoo ifoo = null;
            IBar ibar = null;
            Foo foo = null;
            Bar bar = null;
            FooBar foobar = null;
            FinalBar finalbar = null;
            object o = null;

            o = (IFoo)bar;  // Noncompliant
            o = (IFoo)ibar;
            o = (Foo)bar; // Compliant; causes compiler error // Error [CS0030] - invalid cast
            o = (Foo)ibar;
            o = (IFoo)finalbar; // Compliant; causes compiler error // Error [CS0030] - invalid cast
            o = (Bar)generic; // Compliant; causes compiler error // Error [CS0030] - invalid cast

            o = bar  as IFoo;
            o = ibar as IFoo;
            o = ibar as Foo;
            o = generic as Bar;

            o = bar  is IFoo;
            o = ibar is IFoo;
            o = bar  is Foo;
            o = ibar is Foo;
            o = finalbar is IFoo;
            o = generic is Bar;
        }
    }
}
