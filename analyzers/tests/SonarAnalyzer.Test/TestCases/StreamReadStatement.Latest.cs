using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class StreamReadStatement
    {
        public StreamReadStatement(string fileName)
        {
            using var stream = File.Open(fileName, FileMode.Open);
            var result = new byte[stream.Length];
            (int a, int b) = (stream.Read(result, 0, (int)stream.Length), 42); // Compliant The result is assigned to a
            _ = stream.Read(result, 0, (int)stream.Length);                    // FN. The result is discarded
            (_, var c) = (stream.Read(result, 0, (int)stream.Length), 42);     // FN. The result is discarded
            _ = stream.Read(result, 0, (int)stream.Length) is { };             // FN. The result is discarded
            _ = stream.Read(result, 0, (int)stream.Length) is { } _;           // FN. The result is discarded
            _ = (stream.Read(result, 0, (int)stream.Length), 42) is (_, _);    // FN. The result is discarded
        }

        public void ReadAtLeast(string fileName)
        {
            using var stream = File.Open(fileName, FileMode.Open);
            var result = new byte[stream.Length];
            var i = stream.ReadAtLeast(result, (int)stream.Length);            // Compliant The result is assigned to i
            stream.ReadAtLeast(result, (int)stream.Length);                    // Noncompliant {{Check the return value of the 'ReadAtLeast' call to see how many bytes were read.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        public async Task ReadAtLeastAsync(string fileName)
        {
            using var stream = File.Open(fileName, FileMode.Open);
            var result = new byte[stream.Length];
            var i = await stream.ReadAtLeastAsync(result, (int)stream.Length); // Compliant
            await stream.ReadAtLeastAsync(result, (int)stream.Length);         // Noncompliant {{Check the return value of the 'ReadAtLeastAsync' call to see how many bytes were read.}}
        }
    }
}
