using System.Collections.Generic;
using System.Linq;
using System;

public static class Enumerable
{
    // Extension block
    extension<TSource>(IEnumerable<TSource> source) // extension members for IEnumerable<TSource>
    {
        public bool IsEmpty => !source.Any();       // Compliant
        public bool HasCount => source.Count() > 0; // Noncompliant
    }
}
