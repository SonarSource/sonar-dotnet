using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class CastOnNullable
    {
        public static IEnumerable<string> UsefulCast()
        {
            var nullableStrings = new string?[] { "one", "two", null, "three" };
            return nullableStrings.OfType<string>(); // Noncompliant FP - filters out the null
        }
     }
}
