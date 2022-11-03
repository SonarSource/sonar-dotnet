using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IInvalidCases
    {
        public static virtual IEnumerable<string> Foo(string something) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the iterator.}}
        {
            if (something == null) { throw new ArgumentNullException(nameof(something)); } // Secondary

            yield return something;
        }
    }
}
