using System;

namespace Tests.Diagnostics
{
    interface IInterface
    {
        static abstract void FooBar();
    }

    public class SomeClass : IInterface
    {
        static void FooBar()
        {
            throw new NotImplementedException(); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }
    }
}
