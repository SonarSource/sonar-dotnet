using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{
    public class StreamReadStatement
    {
        public StreamReadStatement(string fileName)
        {
            using (var stream = File.Open(fileName, FileMode.Open))
            {
                var result = new byte[stream.Length];
                stream.Read(result, 0, (int)stream.Length); // Noncompliant
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                var l = stream.Read(result, 0, (int)stream.Length);
                stream.ReadAsync(result, 0, (int)stream.Length); // Noncompliant {{Check the return value of the 'ReadAsync' call to see how many bytes were read.}}
                await stream.ReadAsync(result, 0, (int)stream.Length); // Noncompliant
                stream.Write(result, 0, (int)stream.Length);
            }
        }
    }

    public class DerivedStream : Stream
    {
        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public void Read(byte[] buffer, int offset, string count) { /* do something */ }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }

    public class Program
    {
        public void Foo()
        {
            var stream = new DerivedStream();
            var array = new byte[10];
            stream.Read(array, 0, ""); // Compliant
            stream.Read(array, 0, 10); // Noncompliant
            stream.ReadAsync(array, 0, (int)stream.Length); // Noncompliant {{Check the return value of the 'ReadAsync' call to see how many bytes were read.}}
            await stream.ReadAsync(array, 0, (int)stream.Length); // Noncompliant
            var res = await stream.ReadAsync(array, 0, (int)stream.Length); // Compliant

            stream.ReadAsync(array, 0, (int)stream.Length).Result; // Compliant - (false negative) should not be compliant
        }
    }
}
