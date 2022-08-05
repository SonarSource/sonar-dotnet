using System;

namespace Tests.Diagnostics
{
    class FooNotIDisposable
    {
        Action x = () => GC.SuppressFinalize(new object()); // Noncompliant

        void Foo()
        {
            GC.SuppressFinalize(this); // Noncompliant {{Do not call 'GC.SuppressFinalize'.}}
//             ^^^^^^^^^^^^^^^^
        }
    }

    class FooDisposable : IDisposable
    {
        void Foo()
        {
            GC.SuppressFinalize(this); // Noncompliant
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Compliant
        }

        protected virtual void Dispose(bool disposing)
        {
            GC.SuppressFinalize(this); // Noncompliant
        }
    }
}
