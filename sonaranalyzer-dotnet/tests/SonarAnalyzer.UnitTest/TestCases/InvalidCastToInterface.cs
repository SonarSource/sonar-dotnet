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
        { }

        static void Main()
        {
            var myclass1 = new MyClass1();
            var x = (IMyInterface)myclass1; // Noncompliant
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
            var c = (IMyInterface2)i; // Compliant
            var d = (IMyInterface3)i;

            var o = (object)true;
            d = (IMyInterface3)o;

            var coll = (IEnumerable<int>)new List<int>();

            var z = (IDisposable)new MyClass4();

            var w = (IDisposable)(new Node());
        }
    }

    public class DerivedNode : MiddleNode, IDisposable
    {

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

    public class NullableTest
    {
        public void Test1()
        {
            int? i1 = null;
            var ii = (int)i1; // Noncompliant {{Nullable is known to be empty, this cast throws an exception.}}
//                   ^^^^^^^
        }
        public void Test2()
        {
            int? i2 = 10;
            var ii = (int)i2;
        }
        public void Test3()
        {
            int? i3 = null;
            var d = (double)i3; // Noncompliant
        }
        public void Test4()
        {
            int? i4 = null;
            var n = (NullableTest)i4; // don't care, custom cast
        }
        public void Test5()
        {
            int? i5 = null;
            var d = (double?)i5; // Compliant as the resulting type allows null
        }
        public void Test6()
        {
            int? i6 = 42;
            var d = (double?)i6;
        }
        public void Test7()
        {
            int i7 = 42;
            var d = (double)i7;
        }

        public void TestMethod(object obj)
        {
            var a = obj as MyClass;
            var test = (ushort?)a?.D;
        }

        public static explicit operator NullableTest(int? i)
        {
            return null;
        }
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
            IFoo ifoo;
            IBar ibar;
            Foo foo;
            Bar bar;
            FooBar foobar;
            FinalBar finalbar;
            object o;

            o = (IFoo)bar;  // Noncompliant
            o = (IFoo)ibar;
            o = (Foo)bar; // Compliant; causes compiler error
            o = (Foo)ibar;
            o = (IFoo)finalbar; // Compliant; causes compiler error
            o = (IFoo)T;
            o = (T)IFoo;
            o = (Bar)generic; // Compliant; causes compiler error

            o = bar  as IFoo;
            o = ibar as IFoo;
            o = bar  as Foo;
            o = ibar as Foo;
            o = finalbar as IFoo;
            o = T as IFoo;
            o = IFoo as T;
            o = generic as Bar;

            o = bar  is IFoo;
            o = ibar is IFoo;
            o = bar  is Foo;
            o = ibar is Foo;
            o = finalbar is IFoo;
            o = T is IFoo;
            o = IFoo is T;
            o = generic is Bar;
        }
    }
}