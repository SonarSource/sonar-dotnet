using System;

namespace Tests.Diagnostics
{
    class MyException : NotImplementedException { }

    class Program
    {
        void Foo()
        {
            throw new NotImplementedException(); // Noncompliant {{Implement this method or throw 'NotSupportedException' instead.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        void Bar()
        {
            throw new MyException(); // Compliant - we don't check inheritance
        }

        void FooBar()
        {
            var ex = new NotImplementedException(); // Compliant - not thrown
        }

        void FooBar2()
        {
            var ex = new NotImplementedException();
            throw ex; // Noncompliant
//          ^^^^^^^^^
        }
    }
}
