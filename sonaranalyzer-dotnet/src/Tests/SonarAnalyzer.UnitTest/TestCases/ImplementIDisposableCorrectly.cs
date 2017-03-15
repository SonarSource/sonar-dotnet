using System;

namespace Tests.Diagnostics
{
    public class Foo1 : IDisposable 
//               ^^^^ Noncompliant [0] {{Fix this implementation of IDisposable to conform to the dispose pattern.}}
//               ^^^^ Secondary@-1 [0] {{Provide protected overridable implementation of Dispose(bool) on Foo1 or mark the type as sealed.}}
    {
        public void Dispose()
//                  ^^^^^^^ Secondary [0] {{Foo1.Dispose() should contain only a call to Foo1.Dispose(true) and if the class contains a finalizer, call to GC.SuppressFinalize(this).}}
        {
            // Cleanup
        }

        public virtual void Dispose(bool a, bool b) // This should not affect the implementation
        {
        }
    }

    public class Foo2 : IDisposable // Noncompliant [1]
    {
        void IDisposable.Dispose() 
//                       ^^^^^^^ Secondary [1] {{Foo2.Dispose() should be public.}}
        {
            Dispose(true);
        }

        public virtual void Dispose()
//             ^^^^^^^ Secondary [1] {{Foo2.Dispose() should be sealed.}}
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }

    public class Foo3 : IDisposable // Noncompliant [2]
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        ~Foo3()
//       ^^^^ Secondary [2] {{Modify Foo3.~Foo3() so that it calls Dispose(false) and then returns.}}
        {
            // Cleanup
        }
    }

    public sealed class Foo4 : IDisposable
    {
        public void Dispose()
        {
            // Cleanup
        }
    }

    public class Foo5 : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }
    }

    public class Foo5_1 : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }
    }

    public class Foo6 : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        ~Foo6()
        {
            Dispose(false);
        }
    }

    public class Foo7 : Foo1 // Compliant
    {
    }

    public class Foo8 : Foo5 // Compliant
    {
    }

    public interface IMyDisposable : IDisposable
    {
    }

    public class Foo9 : Foo1, IMyDisposable
//               ^^^^ Noncompliant [3]
//                            ^^^^^^^^^^^^^ Secondary@-1 [3] {{Remove IDisposable from the list of interfaces implemented by Foo9 and override the base class Dispose implementation instead.}}
    {
    }
}
