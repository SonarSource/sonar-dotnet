using System;
using System.Runtime.InteropServices;

namespace MyLibrary
{
    namespace RspecExample
    {
        public class Foo : IDisposable // Noncompliant {{Implement a finalizer that calls your 'Dispose' method.}}
//                   ^^^
        {
            private IntPtr myResource;
            private bool disposed = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    // Dispose of resources held by this instance.
                    FreeResource(myResource);
                    disposed = true;

                    // Suppress finalization of this disposed instance.
                    if (disposing)
                    {
                        GC.SuppressFinalize(this);
                    }
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }
        }
        public class Foo_Compliant : IDisposable
        {
            private IntPtr myResource;
            private bool disposed = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    // Dispose of resources held by this instance.
                    FreeResource(myResource);
                    disposed = true;

                    // Suppress finalization of this disposed instance.
                    if (disposing)
                    {
                        GC.SuppressFinalize(this);
                    }
                }
            }
            ~Foo_Compliant()
            {
                Dispose(false);
            }
        }
    }

    public class Foo_01 : IDisposable // Noncompliant
    {
        private UIntPtr myResource;
    }

    public class Foo_02 : IDisposable // Noncompliant
    {
        private UIntPtr myResource;
    }

    public class Foo_03 : IDisposable // Noncompliant
    {
        private HandleRef myResource;
    }

    public class Foo_04 : IDisposable // Noncompliant
    {
        private HandleRef myResource;

        public void Dispose() {}
    }
    public class Foo_05 : IDisposable
    {
        private HandleRef;
    }

    public class Foo_06 : IDisposable
    {
        private object myResource;
    }

    public class Foo_07
    {
        private HandleRef myResource;

    }
}