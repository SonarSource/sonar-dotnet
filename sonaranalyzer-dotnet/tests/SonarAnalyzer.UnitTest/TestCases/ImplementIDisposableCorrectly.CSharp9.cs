using System;

namespace Tests.Diagnostics
{
    public sealed record SealedDisposable : IDisposable
    {
        public void Dispose() { }
    }

    public record SimpleDisposable : IDisposable // FN, should be Non-compliant with 'SimpleDisposable.Dispose()' should also call 'GC.SuppressFinalize(this)'.
    {
        public void Dispose() => Dispose(true); // FN, should be second location with 'SimpleDisposable.Dispose()' should also call 'GC.SuppressFinalize(this)'.

        protected virtual void Dispose(bool disposing) { }
    }

    public record SimpleDisposableWithSuppressFinalize : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public record DisposableWithMoreThanTwoStatements : IDisposable // FN, should be Non-compliant
    {
        public void Dispose() // FN, should be second location with 'DisposableWithMoreThanTwoStatements.Dispose()' should call 'Dispose(true)', 'GC.SuppressFinalize(this)' and nothing else.
        {
            Dispose(true);
            Console.WriteLine("Extra statement");
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public record DerivedDisposable : SimpleDisposable
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public record FinalizedDisposable : IDisposable
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

    public record FinalizedDisposableExpression : IDisposable
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

    public record NoVirtualDispose : IDisposable
// FN, should be Non-compliant with message: Fix this implementation of 'IDisposable' to conform to the dispose pattern.
// FN, should be second location with @ -1 Provide 'protected' overridable implementation of 'Dispose(bool)' on 'NoVirtualDispose' or mark the type as 'sealed'.
    {
        public void Dispose() { }
// FN, should be second location with 'NoVirtualDispose.Dispose()' should call 'Dispose(true)' and 'GC.SuppressFinalize(this)'.

        public virtual void Dispose(bool a, bool b) { } // This should not affect the implementation
    }

    public record ExplicitImplementation : IDisposable // FN, should be Non-compliant
    {
        void IDisposable.Dispose()
// FN, should be second location with 'ExplicitImplementation.Dispose()' should also call 'GC.SuppressFinalize(this)'.
// FN, should be second location with @ -1 'ExplicitImplementation.Dispose()' should be 'public'.
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public record VirtualImplementation : IDisposable // FN, should be Non-compliant
    {
        public virtual void Dispose()
// FN, should be second location with 'VirtualImplementation.Dispose()' should not be 'virtual' or 'abstract'.
// FN, should be second location with @ -1 'VirtualImplementation.Dispose()' should also call 'GC.SuppressFinalize(this)'.
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }
    }

    public record WithFinalizer : IDisposable // Non-compliant
    {
        public void Dispose()
// FN, should be second location with 'WithFinalizer.Dispose()' should also call 'GC.SuppressFinalize(this)'.
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }

        ~WithFinalizer() { }
// FN, should be second location with Modify 'WithFinalizer.~WithFinalizer()' so that it calls 'Dispose(false)' and then returns.
    }

    public record WithFinalizer2 : IDisposable // Non-compliant
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }

        ~WithFinalizer2() // second location with, more than one line
        {
            Dispose(false);
            Console.WriteLine(1);
            Console.WriteLine(2);
        }
    }

    public record DerivedDisposable1 : NoVirtualDispose // Compliant, we are not in charge of our base
    {
    }

    public record DerivedDisposable2 : SimpleDisposable // Compliant, we do not override Dispose(bool)
    {
    }

    public record DisposeNotCallingBase1 : SimpleDisposable // FN, should be Non-compliant
    {
        protected override void Dispose(bool disposing) { }
// FN, should be second location with Modify 'Dispose(disposing)' so that it calls 'base.Dispose(disposing)'.
    }

    public record DisposeNotCallingBase2 : DerivedDisposable2 // FN, should be Non-compliant, checking for deeper inheritance here
    {
        protected override void Dispose(bool disposing)
// FN, should be second location with Modify 'Dispose(disposing)' so that it calls 'base.Dispose(disposing)'.
        {
        }
    }

    public interface IMyDisposable : IDisposable // Compliant, interface
    {
    }

    public record DerivedWithInterface1 : NoVirtualDispose, IDisposable
// FN, should be Non-compliant
// FN, should be second location with @ -1 Remove 'IDisposable' from the list of interfaces implemented by 'DerivedWithInterface1' and override the base class 'Dispose' implementation instead.
    {
    }

    public record DerivedWithInterface2 : NoVirtualDispose, IMyDisposable // Compliant, we are not in charge of the interface
    {
    }
}
