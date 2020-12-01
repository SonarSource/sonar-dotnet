using System;
using System.Collections;

namespace Tests.Diagnostics
{
    class Program
    {
        public Program(object o)
        {
            if (this is not IDisposable) // FN
            {
            }

            switch (this) // FN
            {
                case IDisposable:
                    break;
                case IEnumerable:
                    break;
                default:
                    break;
            }

            var result = this switch // FN
            {
                IDisposable => 1,
                IEnumerable => 2,
                _ => 3
            };
        }
    }

    record R
    {
        void Foo()
        {
            if (this is IDisposable) // Noncompliant {{Offload the code that's conditional on this 'is' test to the appropriate subclass and remove the test.}}
//              ^^^^^^^^^^^^^^^^^^^
            {
            }
        }
    }
}
