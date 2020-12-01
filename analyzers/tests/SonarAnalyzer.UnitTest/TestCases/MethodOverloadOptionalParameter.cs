using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        void Print2(string[] messages);
        void Print2(string[] messages, string delimiter = "\n");// Noncompliant;
//                                     ^^^^^^^^^^^^^^^^^^^^^^^
    }

    public class MyBase
    {
        public virtual void Print3(string[] messages) { }
        public virtual void Print3(string[] messages, string delimiter = "\n") { }// Noncompliant {{This method signature overlaps the one defined on line 15, the default parameter value can't be used.}}
    }

    public partial class MethodOverloadOptionalParameter : MyBase, IMyInterface
    {
        public override void Print3(string[] messages) { }
        public override void Print3(string[] messages, string delimiter = "\n") { }// Compliant; comes from base class

        public void Print2(string[] messages) { }
        public void Print2(string[] messages, string delimiter = "\n") { } // Compliant; comes from interface

        partial void Print(string[] messages);

        partial void Print(string[] messages) { }

        void Print(string[] messages, string delimiter = "\n") {} // Noncompliant;
        void Print(string[] messages,
            string delimiter = "\n", // Noncompliant {{This method signature overlaps the one defined on line 29, the default parameter value can only be used with named arguments.}}
            string b = "a" // Noncompliant {{This method signature overlaps the one defined on line 31, the default parameter value can't be used.}}
            ) {}
    }

    public interface MyType
    {
        void Print(string messages, int num, object something);

        void Print(string messages, int num = 0, object something = null); // Error [CS0111] - Already contains member with same params
    }

    public interface Generics
    {
        void Foo<T>(string[] messages);
        void Foo<T>(string[] messages, string delimiter = "\n"); // Noncompliant

        void Foo2(string[] messages);
        void Foo2<T>(string[] messages, string delimiter = "\n");

        void Foo3<T>(T messages);
        void Foo3<V>(V messages, string delimiter = "\n"); // Noncompliant

        void Foo4<T>(IEnumerable<T> messages);
        void Foo4<V>(IEnumerable<V> messages, string delimiter = "\n"); // Noncompliant

        void Foo5<T>(IEnumerable<T> messages);
        void Foo5<MyType>(MyType messages);
        void Foo5(MyType messages, string delimiter = "\n");

        void Foo6<T>(KeyValuePair<T, int> ids);
        void Foo6<U>(KeyValuePair<U, string> messages, string delimiter = "\n");

        void Foo7<T>(KeyValuePair<KeyValuePair<T, int>, string> ids);
        void Foo7<U>(KeyValuePair<KeyValuePair<U, string>, string> messages, string delimiter = "\n");

        void Foo8<T>(KeyValuePair<KeyValuePair<T, int>, string> ids);
        void Foo8<U>(KeyValuePair<KeyValuePair<U, int>, string> messages, string delimiter = "\n"); // Noncompliant
    }

    public interface IInterfaceWithDefaultImpl
    {
        void Print2(string[] messages)
        {
        }

        void Print2(string[] messages, string delimiter = "\n") // Noncompliant;
//                                     ^^^^^^^^^^^^^^^^^^^^^^^
        {
        }
    }
}
