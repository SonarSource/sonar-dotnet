using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class RedundantToArrayCall
    {
        public void Utf8StringLiterals(int propertyValue)
        {
            var c = "some string"u8.ToArray()[10];        // Noncompliant
            c = "some string"u8.Slice(5, 4)[1];           // Compliant
            foreach (var v in "some string"u8.ToArray())  // Noncompliant {{Remove this redundant 'ToArray' call.}}
            {
                // ...
            }

            var x = "some string"u8.ToArray(); // Compliant
        }

        public void ReadOnlySpans(int propertyValue)
        {
            unsafe
            {
                void* pointer = null;

                var span = new ReadOnlySpan<byte>(pointer, 42);

                var elementAccess = span.ToArray()[10];        // Noncompliant
                var sliced = span.Slice(5, 4)[1];           // Compliant

                foreach (var v in span.ToArray())  // Noncompliant {{Remove this redundant 'ToArray' call.}}
                {
                    // ...
                }

                var arr = span.ToArray(); // Compliant
            }
        }
    }
}
