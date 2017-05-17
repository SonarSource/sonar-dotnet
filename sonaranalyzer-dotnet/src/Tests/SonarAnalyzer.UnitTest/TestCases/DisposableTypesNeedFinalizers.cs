using System;
using System.Runtime.InteropServices;

namespace MyLibrary
{
    public class Foo : IDisposable // Noncompliant {{Implement a finalizer that calls your 'Dispose' method.}}
//               ^^^
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

    public class Foo2 : IDisposable // Noncompliant
    {
        private UIntPtr myResource;

        public void Dispose()
        {
        }
    }


    public class Foo3 : IDisposable // Noncompliant
    {
        private HandleRef myResource;

        public void Dispose()
        {
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


    public class Foo_Compliant2 : IDisposable
    {
        private object myResource;

        public void Dispose()
        {
        }
    }
}