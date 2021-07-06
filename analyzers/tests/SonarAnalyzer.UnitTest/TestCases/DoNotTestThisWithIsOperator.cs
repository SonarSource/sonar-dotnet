using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public Program(object o)
        {
            if (this is IDisposable) // Noncompliant {{Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.}}
//              ^^^^^^^^^^^^^^^^^^^
            {
            }

            if (((((this)))) is IDisposable) // Noncompliant {{Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
            }

            if (o is IDisposable)
            {
            }
        }
    }
}
