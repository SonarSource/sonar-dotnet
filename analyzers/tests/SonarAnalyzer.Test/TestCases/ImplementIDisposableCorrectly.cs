using System;

namespace Tests.Diagnostics
{
    public sealed class SealedDisposable : IDisposable
    {
        public void Dispose() { }
    }

    public class SimpleDisposable : IDisposable // Noncompliant {{Fix this implementation of 'IDisposable' to conform to the dispose pattern.}}
    {
        public void Dispose() => Dispose(true); // Secondary {{'SimpleDisposable.Dispose()' should also call 'GC.SuppressFinalize(this)'.}}

        protected virtual void Dispose(bool disposing) { }
    }

    public class SimpleDisposableWithSuppressFinalize : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public class DisposableWithMoreThanTwoStatements : IDisposable // Noncompliant
    {
        public void Dispose() // Secondary {{'DisposableWithMoreThanTwoStatements.Dispose()' should call 'Dispose(true)', 'GC.SuppressFinalize(this)' and nothing else.}}
        {
            Dispose(true);
            Console.WriteLine("Extra statement");
            GC.SuppressFinalize(this);
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

    public class FinalizedDisposableExpression : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

        ~FinalizedDisposableExpression() =>
            Dispose(false);
    }

    public class NoVirtualDispose : IDisposable
//               ^^^^^^^^^^^^^^^^ Noncompliant {{Fix this implementation of 'IDisposable' to conform to the dispose pattern.}}
//               ^^^^^^^^^^^^^^^^ Secondary@-1 {{Provide 'protected' overridable implementation of 'Dispose(bool)' on 'NoVirtualDispose' or mark the type as 'sealed'.}}
    {
        public void Dispose() { }
//                  ^^^^^^^ Secondary {{'NoVirtualDispose.Dispose()' should call 'Dispose(true)' and 'GC.SuppressFinalize(this)'.}}

        public virtual void Dispose(bool a, bool b) { } // This should not affect the implementation
    }

    public class ExplicitImplementation : IDisposable // Noncompliant
    {
        void IDisposable.Dispose()
//                       ^^^^^^^ Secondary {{'ExplicitImplementation.Dispose()' should also call 'GC.SuppressFinalize(this)'.}}
//                       ^^^^^^^ Secondary@-1 {{'ExplicitImplementation.Dispose()' should be 'public'.}}
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public class VirtualImplementation : IDisposable // Noncompliant
    {
        public virtual void Dispose()
//             ^^^^^^^ Secondary {{'VirtualImplementation.Dispose()' should not be 'virtual' or 'abstract'.}}
//                          ^^^^^^^ Secondary@-1 {{'VirtualImplementation.Dispose()' should also call 'GC.SuppressFinalize(this)'.}}
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public class WithFinalizer : IDisposable // Noncompliant
    {
        public void Dispose()
//                  ^^^^^^^ Secondary {{'WithFinalizer.Dispose()' should also call 'GC.SuppressFinalize(this)'.}}
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

    public class SimpleDisposableCompilationError : IDisposable  // Noncompliant {{Fix this implementation of 'IDisposable' to conform to the dispose pattern.}}
    {
        public void Dispose() => Dispose(true, false);           // Error [CS1501]
                                                                 // Secondary@-1 {{'SimpleDisposableCompilationError.Dispose()' should call 'Dispose(true)' and 'GC.SuppressFinalize(this)'.}}

        protected virtual void Dispose(bool disposing) { }
    }
}

namespace Rspec_Compliant_Samples
{
    // Sealed class
    public sealed class Foo1 : IDisposable
    {
        public void Dispose()
        {
            // Cleanup
        }
    }

    // Simple implementation
    public class Foo2 : IDisposable
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
    }

    // Implementation with a finalizer
    public class Foo3 : IDisposable
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
        {
            Dispose(false);
        }
    }

    // Base disposable class
    public class Foo4 : Foo2
    {
        protected override void Dispose(bool disposing)
        {
            // Cleanup
            // Do not forget to call base
            base.Dispose(disposing);
        }
    }

    // Base disposable class, expression body
    public class Foo5 : Foo2
    {
        protected override void Dispose(bool disposing) =>
            // Cleanup
            // Do not forget to call base
            base.Dispose(disposing);
    }

    public ref struct Struct
    {
        public void Dispose()
        {
        }
    }

    public ref struct Struct2
    {
        public void Dispose(bool disposing) // Compliant - FN: for ref structs the pattern is to have Dispose without parameters
        {
        }
    }
}

namespace VS_Generated_Implementation
{
    // NOTE: this is not compliant
    public class Foo3 : IDisposable // Noncompliant
    {

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Foo3() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() // Secondary
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
