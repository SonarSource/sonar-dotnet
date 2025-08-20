using System;
using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{
    public interface IMine
    {
        void Dispose(); //Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
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

    public class NoRealDisposalDone : IDisposable
    {
        private void Dummy() { }

        public void Dispose()
        {
            Dummy();
            NotExists();  // Error [CS0103]
        }
    }

    public class GarbageDisposalException2 : SomeUnknownType // Error [CS0246] - unknown type
    {
        protected override void Dispose(bool disposing)
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

    public partial class MyPartial
    {
    }

    public partial class MyPartial
    {
        void Dispose() { }  // Noncompliant
    }

    public ref struct RefStruct
    {
        public void Dispose() // ok
        {
        }
    }

    public struct Struct
    {
        public void Dispose() // Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
        {
        }
    }

    public struct DisposableStruct : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9679
namespace Repro_9679
{
    public static class DisposableExtensions
    {
        public static void Dispose<T>(this Lazy<T> lazy) // Compliant
            where T : class, IDisposable
        {
            if (lazy.IsValueCreated)
            {
                lazy.Value.Dispose();
            }
        }
    }
}

// https://sonarsource.atlassian.net/browse/NET-2257
namespace Repro_NET_2257
{
    public class Parent : IDisposable
    {
        public virtual void Dispose()
        {
            // Do nothing
        }
    }

    public class Child : Parent
    {
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool flag)  // Noncompliant FP, if the user has no access to the parent class, they have no choice but to implement this method in their own class
        {
            // Do nothing
        }
    }
}
