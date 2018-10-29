using System;
using System.Collections.Generic;
using System.IO;

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

    public class Mine0 : IDisposable
    {
        public void Dispose() { }
    }

    public class Mine1 : IMine
    {
        public void Dispose() { }
    }
    public class Mine2 : IMine2
    {
        public void Dispose() { }
    }

    public class Mine3 : ISomeUnknown // Error [CS0246] - unknown type
    {
        public void Dispose() { }
    }

    public class Mine4 : ISomeUnknown, ISomeUnknown2 // Error [CS0246,CS0246] - unknown type
    {
        public void Dispose() { }
    }

    public class GarbageDisposal
    {
        private int Dispose()  // Noncompliant
        {
            // ...
            return 42;
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

    public class GarbageDisposalException2 : SomeUnknownType // Error [CS0246] - unknown type
    {
        protected override void Dispose(bool disposing) // Error [CS0115] - no method to override
        {
            //...
        }
    }

    public class MyStream : Stream
    {
        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

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
