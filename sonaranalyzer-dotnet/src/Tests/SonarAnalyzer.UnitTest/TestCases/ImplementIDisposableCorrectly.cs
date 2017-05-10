using System;

namespace Tests.Diagnostics
{
    public sealed class SealedDisposable : IDisposable
    {
        public void Dispose() { }
    }

    public class SimpleDisposable : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public class DerivedDisposable : SimpleDisposable
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class FinalizedDisposable : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

        ~FinalizedDisposable()
        {
            Dispose(false);
        }
    }

    public class NoVirtualDispose : IDisposable
//               ^^^^^^^^^^^^^^^^ Noncompliant {{Fix this implementation of 'IDisposable' to conform to the dispose pattern.}}
//               ^^^^^^^^^^^^^^^^ Secondary@-1 {{Provide 'protected' overridable implementation of 'Dispose(bool)' on 'NoVirtualDispose' or mark the type as 'sealed'.}}
    {
        public void Dispose() { }
//                  ^^^^^^^ Secondary {{'NoVirtualDispose.Dispose()' should contain only a call to 'Dispose(true)' and if the class contains a finalizer, call to 'GC.SuppressFinalize(this)'.}}

        public virtual void Dispose(bool a, bool b) { } // This should not affect the implementation
    }

    public class ExplicitImplementation : IDisposable // Noncompliant
    {
        void IDisposable.Dispose()
//                       ^^^^^^^ Secondary {{'ExplicitImplementation.Dispose()' should be 'public'.}}
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public class VirtualImplementation : IDisposable // Noncompliant
    {
        public virtual void Dispose()
//             ^^^^^^^ Secondary {{'VirtualImplementation.Dispose()' should not be 'virtual' or 'abstract'.}}
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public class WithFinalizer : IDisposable // Noncompliant
    {
        public void Dispose()
//                  ^^^^^^^ Secondary {{'WithFinalizer.Dispose()' should contain only a call to 'Dispose(true)' and if the class contains a finalizer, call to 'GC.SuppressFinalize(this)'.}}
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }

        ~WithFinalizer() { }
//       ^^^^^^^^^^^^^ Secondary {{Modify 'WithFinalizer.~WithFinalizer()' so that it calls 'Dispose(false)' and then returns.}}
    }

    public class WithFinalizer2 : IDisposable // Noncompliant
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

        ~WithFinalizer2() // Secondary, more than one line
        {
            Dispose(false);
            Console.WriteLine(1);
            Console.WriteLine(2);
        }
    }

    public class DerivedDisposable1 : NoVirtualDispose // Compliant, we are not in charge of our base
    {
    }

    public class DerivedDisposable2 : SimpleDisposable // Compliant, we do not override Dispose(bool)
    {
    }

    public class DisposeNotCallingBase1 : SimpleDisposable // Noncompliant
    {
        protected override void Dispose(bool disposing) { }
//                              ^^^^^^^ Secondary {{Modify 'Dispose(disposing)' so that it calls 'base.Dispose(disposing)'.}}
    }

    public class DisposeNotCallingBase2 : DerivedDisposable2 // Noncompliant, checking for deeper inheritance here
    {
        protected override void Dispose(bool disposing)
//                              ^^^^^^^ Secondary {{Modify 'Dispose(disposing)' so that it calls 'base.Dispose(disposing)'.}}
        {
        }
    }


    public interface IMyDisposable : IDisposable // Compliant, interface
    {
    }

    public class DerivedWithInterface1 : NoVirtualDispose, IDisposable
//               ^^^^^^^^^^^^^^^^^^^^^ Noncompliant
//                                                         ^^^^^^^^^^^ Secondary@-1 {{Remove 'IDisposable' from the list of interfaces implemented by 'DerivedWithInterface1' and override the base class 'Dispose' implementation instead.}}
    {
    }

    public class DerivedWithInterface2 : NoVirtualDispose, IMyDisposable // Compliant, we are not in charge of the interface
    {
    }
}
