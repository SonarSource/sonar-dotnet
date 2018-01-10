using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IMine
    {
        void Dispose(); //Noncompliant
//           ^^^^^^^
    }

    public interface IMine2 : IDisposable
    {
        void Dispose(); //Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
    }

    public class Mine3 : IDisposable
    {
        public void Dispose() { }
    }

    public class Mine : IMine
    {
        public void Dispose() { }
    }
    public class Mine2 : IMine2
    {
        public void Dispose() { }
    }

    public class Mine3 : ISomeUnknown
    {
        public void Dispose() { }
    }

    public class Mine4 : ISomeUnknown, ISomeUnknown2
    {
        public void Dispose() { }
    }

    public class GarbageDisposal
    {
        private int Dispose()  // Noncompliant
        {
            // ...
        }

        private void Dummy()
        {

        }
    }
    public class GarbageDisposalExceptionBase : IDisposable
    {
        protected virtual void Dispose(bool disposing)
        {
            //...
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class GarbageDisposalException : GarbageDisposalExceptionBase
    {
        protected override void Dispose(bool disposing)
        {
            //...
        }
    }

    public class GarbageDisposalException2 : SomeUnknownType
    {
        protected override void Dispose(bool disposing)
        {
            //...
        }
    }

    public class MyStream : System.IO.Stream
    {
        protected override void Dispose(bool disposing)
        {

        }
    }

    public partial class MyPartial : IDisposable
    {
        public void Dispose()
        {
            // Dispose(10)
        }
    }

    public partial class MyPartial
    {
        public void Dispose(int i) // Non-compliant, but not reported now because of the partial
        {

        }
    }
}
