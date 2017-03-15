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
            x = myclass1 as IMyInterface; // Noncompliant
//                          ^^^^^^^^^^^^
            bool b = myclass1 is IMyInterface; // Noncompliant {{Review this cast; in this project there's no type that extends 'MyClass1' and implements 'IMyInterface'.}}

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

    public class NullableTest
    {
        public void Test1()
        {
            int? i = null;
            var ii = (int)i; // Noncompliant {{Nullable is known to be empty, this cast throws an exception.}}
//                   ^^^^^^
}
        public void Test2()
        {
            int? i = 10;
            var ii = (int)i;
        }
        public void Test3()
        {
            int? i = null;
            var d = (double)i; // Noncompliant
        }
        public void Test4()
        {
            int? i = null;
            var n = (NullableTest)i; // don't care, custom cast
        }

        public static explicit operator NullableTest(int? i)
        {
            return null;
        }
    }
}