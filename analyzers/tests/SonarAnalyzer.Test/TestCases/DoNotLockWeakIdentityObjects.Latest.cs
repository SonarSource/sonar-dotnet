using System;
using System.Threading;

namespace Tests.Diagnostics
{
    class Program
    {
        private string rawStringLiterals = """some value""";

        private void Test()
        {
            lock (rawStringLiterals) { } // Noncompliant
        }
    }

    public class LockOnLockType
    {
        private Lock _lock;
        public void LockOnLock()
        {
            lock (_lock) // Compliant
            { }
        }
    }
}

namespace CSharp15
{
    using System.Buffers;
    using System.IO;
    using System.Text;

    public class NewStreamTypes
    {
        private readonly ReadOnlyMemoryStream readOnlyMemoryStream = new(new byte[1]);
        private readonly WritableMemoryStream writableMemoryStream = new(new byte[1]);
        private readonly StringStream stringStream = new("text", Encoding.UTF8);
        private readonly ReadOnlySequenceStream readOnlySequenceStream = new(ReadOnlySequence<byte>.Empty);

        public void LockOnStreams()
        {
            lock (readOnlyMemoryStream) { }     // Noncompliant {{Replace this lock on 'ReadOnlyMemoryStream' with a lock against an object that cannot be accessed across application domain boundaries.}}
            lock (writableMemoryStream) { }     // Noncompliant
            lock (stringStream) { }             // Noncompliant
            lock (readOnlySequenceStream) { }   // Noncompliant
        }

        public void LockOnExtensionIndexer(Bag bag)
        {
            lock (bag[0]) { }                   // Compliant
            lock (bag["name"]) { }              // Noncompliant {{Replace this lock on 'String' with a lock against an object that cannot be accessed across application domain boundaries.}}
            lock (bag[0L]) { }                  // Noncompliant {{Replace this lock on 'Stream' with a lock against an object that cannot be accessed across application domain boundaries.}}
        }

        public void LockOnGenericExtensionIndexer()
        {
            lock (new GenericBag<string>()[0]) { }  // Noncompliant {{Replace this lock on 'String' with a lock against an object that cannot be accessed across application domain boundaries.}}
            lock (new GenericBag<Thread>()[0]) { }  // Noncompliant {{Replace this lock on 'Thread' with a lock against an object that cannot be accessed across application domain boundaries.}}
            lock (new GenericBag<object>()[0]) { }  // Compliant
        }
    }

    public class Bag { }

    public class GenericBag<T> { }

    public static class Extensions
    {
        extension(Bag bag)
        {
            public object this[int index] => null;
            public string this[string name] => name;
            public Stream this[long index] => null;
        }

        extension<T>(GenericBag<T> bag)
        {
            public T this[int index] => default;
        }
    }
}
