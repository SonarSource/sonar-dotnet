using System;
using System.Collections;
using System.Collections.Generic;

class InlineArrays
{
    IEnumerable<int> Invalid(Buffer? buffer)     // Noncompliant
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer)); // Secondary

        foreach (var item in new Buffer())
            yield return item;
    }

    IEnumerable<int> Valid(Buffer? buffer)       // Compliant
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        return Generator(buffer.Value);

        IEnumerable<int> Generator(Buffer buffer)
        {
            foreach (var item in buffer)
                yield return item;
        }
    }

    [System.Runtime.CompilerServices.InlineArray(10)]
    struct Buffer
    {
        int arrayItem;
    }
}
