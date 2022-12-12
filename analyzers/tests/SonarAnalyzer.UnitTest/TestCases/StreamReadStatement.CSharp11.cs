using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class StreamReadStatement
    {
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
