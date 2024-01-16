using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class RedundantToArrayCall
    {
        public void Utf8StringLiterals()
        {
            var c = "some string"u8[10];        // Fixed
            c = "some string"u8.Slice(5, 4)[1];           // Compliant
            foreach (var v in "some string"u8)  // Fixed
            {
                // ...
            }

            var arr = "some string"u8.ToArray(); // Compliant
        }

        public void ReadOnlySpans(ReadOnlySpan<byte> span)
        {
                var elementAccess = span[10];        // Fixed
                var sliced = span.Slice(5, 4)[1];           // Compliant

                foreach (var v in span)  // Fixed
                {
                    // ...
                }

                var arr = span.ToArray(); // Compliant
        }
    }
}
