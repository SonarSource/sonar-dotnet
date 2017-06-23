using System;

namespace Tests.Diagnostics
{
    public interface IFoo
    {
        void Bar();
    }

    public class Foo : IFoo
    {
        void IFoo.Bar() // Noncompliant
//           ^^^^^^^^
        {
        }
    }

    public sealed class Foo2 : IFoo
    {
        void IFoo.Bar()// Compliant - Foo2 is sealed
        {
        }
    }

    public class Foo3 : IFoo
    {
        void Bar() // Compliant - IFoo is not explicitly implemented
        {
        }
    }

    public class Foo4 : IFoo
    {
        void IFoo.Bar() // Compliant - public method with same name
        {
        }

        public void Bar() { }
    }

    public class Foo5 : IFoo
    {
        void IFoo.Bar() // Compliant - public method with same name
        {
        }

        public int Bar() => 42;
    }

    public class Foo6 : IFoo
    {
        void IFoo.Bar() // Compliant - public method with same name
        {
        }

        public void Bar(int i) { }
    }

    public class Foo7 : IFoo
    {
        void IFoo.Bar() // Noncompliant
        {
        }

        private void Bar() { }
    }

    public class MyClass : IDisposable
    {
        void IDisposable.Dispose()  // Noncompliant
        {
        }
    }

    public class MyClass2 : IDisposable
    {
        void IDisposable.Dispose() // Compliant
        {
        }

        void Dispose() { }
    }

    public class MyClass3 : IDisposable
    {
        void IDisposable.Dispose() // Compliant - Close is an allowed special case for IDisposable
        {
        }

        void Close() { }
    }
}
