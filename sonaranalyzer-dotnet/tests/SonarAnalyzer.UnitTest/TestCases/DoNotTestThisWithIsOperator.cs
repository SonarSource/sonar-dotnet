using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public Program(object o)
        {
            if (this is IDisposable) // Noncompliant {{Offload the code that's conditional on this 'is' test to the appropriate subclass and remove the test.}}
//              ^^^^^^^^^^^^^^^^^^^
            {
            }

            if (o is IDisposable)
            {
            }
        }
    }
}
