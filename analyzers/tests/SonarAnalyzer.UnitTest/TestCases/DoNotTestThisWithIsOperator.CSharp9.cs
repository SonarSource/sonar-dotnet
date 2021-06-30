using System;
using System.Collections;

namespace Tests.Diagnostics
{
    class Program
    {
        public int MyProperty { get; set; }

        public Program(object o)
        {
            if (this is not IDisposable) // Noncompliant {{Offload the code that's conditional on this 'is' test to the appropriate subclass and remove the test.}}
            {
            }

            if (o is not IDisposable)
            {
            }

            if (this is Program p) // Noncompliant
            {
            }

            if (this is Program or IDisposable) // Noncompliant
            {
            }

            if (this is Program and IDisposable) // Noncompliant
            {
            }

            if (this is var variable) // Noncompliant
            {
            }

            if (this is { MyProperty: 5 }) // Noncompliant
            {
            }

            switch (this) // Noncompliant
            {
                case IDisposable:
                    break;
                case IEnumerable:
                    break;
                default:
                    break;
            }

            switch (o)
            {
                case IDisposable:
                    break;
                case IEnumerable:
                    break;
                default:
                    break;
            }

            var result = (((this))) switch // Noncompliant
            {
                IDisposable => 1,
                IEnumerable => 2,
                _ => 3
            };

            result = o switch
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
