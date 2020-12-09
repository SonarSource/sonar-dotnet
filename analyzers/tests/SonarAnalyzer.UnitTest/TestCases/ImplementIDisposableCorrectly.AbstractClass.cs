using System;

namespace Tests.Diagnostics
{
    public abstract class AbstractNotDisposable
    {
        public void Dispose() { } // is ignored
    }

    public abstract class AbstractWithAbstractDispose : IDisposable // Noncompliant
    {
        public abstract void Dispose();
//             ^^^^^^^^ Secondary {{'AbstractWithAbstractDispose.Dispose()' should not be 'virtual' or 'abstract'.}}
        protected abstract void Dispose(bool disposing);
    }

    public abstract class AbstractWithoutProtectedDispose : IDisposable
//                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Fix this implementation of 'IDisposable' to conform to the dispose pattern.}}
//                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary@-1 {{Provide 'protected' overridable implementation of 'Dispose(bool)' on 'AbstractWithoutProtectedDispose' or mark the type as 'sealed'.}}
    {
        public void Dispose() // Secondary {{'AbstractWithoutProtectedDispose.Dispose()' should also call 'Dispose(true)'.}}
        {
            GC.SuppressFinalize(this);
        }
    }

    public abstract class AbstractWithVirtual : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }

    public class DerivedFromAbstractWithVirtual : AbstractWithVirtual
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class DerivedFromAbstractWithVirtualWithExpression : AbstractWithVirtual
    {
        protected override void Dispose(bool disposing) => base.Dispose(disposing);
    }

    public class DerivedFromAbstractWithVirtualNoBaseCall : AbstractWithVirtual // Noncompliant
    {
        protected override void Dispose(bool disposing) { }
//                              ^^^^^^^ Secondary {{Modify 'Dispose(disposing)' so that it calls 'base.Dispose(disposing)'.}}
    }

    public abstract class AbstractWithAbstract : IDisposable // compliant
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }

    public class DerivedFromAbstractWithAbstract : AbstractWithAbstract
    {
        protected override void Dispose(bool disposing)
        {
            // Does not call Base.Dispose(disposing) because the base method is abstract.
        }
    }

}
