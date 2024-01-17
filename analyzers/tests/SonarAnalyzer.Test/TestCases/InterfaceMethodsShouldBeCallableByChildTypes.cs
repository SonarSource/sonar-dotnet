using System;

namespace Tests.Diagnostics
{
    public interface IFoo
    {
        void Method();
        int Property { get; set; }

        event EventHandler Event;
    }

    public class Foo : IFoo
    {
        void IFoo.Method() // Noncompliant {{Make 'Foo' sealed, change to a non-explicit declaration or provide a new method exposing the functionality of 'IFoo.Method'.}}
//                ^^^^^^
        {
        }

        void Method() { }

        int IFoo.Property // Noncompliant {{Make 'Foo' sealed, change to a non-explicit declaration or provide a new method exposing the functionality of 'IFoo.Property'.}}
//               ^^^^^^^^
        { get; set; }

        int Property { get; set; }

        event EventHandler IFoo.Event // Noncompliant {{Make 'Foo' sealed, change to a non-explicit declaration or provide a new method exposing the functionality of 'IFoo.Event'.}}
//                              ^^^^^
        { add { } remove { } }

        event EventHandler Event {  add { } remove { } }
    }

    public sealed class Foo2 : IFoo
    {
        void IFoo.Method() // Compliant - Foo2 is sealed
        {
        }
        int IFoo.Property
        { get; set; }
        event EventHandler IFoo.Event
        { add { } remove { } }
    }

    public class Foo3 : IFoo
    {
        public void Method() // Compliant - IFoo is not explicitly implemented
        {
        }

        public int Property { get; set; }
        public event EventHandler Event { add { } remove { } }
    }

    public class Foo4 : IFoo
    {
        void IFoo.Method() // Compliant - public method with same name, params and return type
        {
        }

        public void Method() { }

        int IFoo.Property
        { get; set; }

        public int Property { get; set; }

        event EventHandler IFoo.Event
        { add { } remove { } }

        public event EventHandler Event { add { } remove { } }
    }

    public class Foo5 : IFoo
    {
        void IFoo.Method() // Compliant - public method with same name, params BUT different return type
        {
        }

        public int Method() => 42;

        int IFoo.Property
        { get; set; }

        public float Property { get; set; }

        event EventHandler IFoo.Event
        { add { } remove { } }

        public event EventHandler<ResolveEventArgs> Event
        { add { } remove { } }
    }

    public class Foo6 : IFoo
    {
        void IFoo.Method() // Compliant - public method with same name, return type BUT different param
        {
        }

        public void Method(int i) { }

        int IFoo.Property
        { get; set; }

        public int Property { get; }

        event EventHandler IFoo.Event
        { add { } remove { } }

        public event EventHandler Event { add { } remove { } }
    }

    public class Foo7 : IFoo
    {
        void IFoo.Method() // Noncompliant
        {
        }

        private void Method() { }

        int IFoo.Property // Noncompliant
        { get; set; }

        private int Property { get; set; }

        event EventHandler IFoo.Event // Noncompliant
        { add { } remove { } }

        private event EventHandler Event { add { } remove { } }
    }

    public class Foo8 : IFoo
    {
        void IFoo.Method() // Compliant
        {
        }

        protected void Method() { }

        int IFoo.Property // Compliant
        { get; set; }

        protected int Property { get; set; }

        event EventHandler IFoo.Event // Compliant
        { add { } remove { } }

        protected event EventHandler Event { add { } remove { } }
    }

    public class MyClass3 : IDisposable
    {
        void IDisposable.Dispose() // Compliant - Close is an allowed special case for IDisposable
        {
        }

        public void Close() { }
    }

    public class MyClass4 : IDisposable
    {
        void IDisposable.Dispose() // Noncompliant
        {
        }

        protected void Bar() { }
    }

    public class MyClass5 : MyDisposable
    {
        void MyDisposable.NotDispose() // Noncompliant
        {
        }

        protected void Bar() { }
    }

    public interface MyDisposable
    {
        void NotDispose();
    }
}
