using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void Foo()
        {
            throw new NotImplementedException(); // Noncompliant {{Implement this method or throw 'NotSupportedException' instead.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }
    }
}
