using System;
using System.Collections;

namespace Tests.Diagnostics
{
    class Program
    {
        public int MyProperty { get; set; }
        public object MyProperty2 { get; set; }

        public Program(object o)
        {
            if (this is Program and { MyProperty: 2 }) // Noncompliant
            {
            }

            if (this is not IDisposable) // Noncompliant {{Offload the code that's conditional on this 'is' test to the appropriate subclass and remove the test.}}
            {
            }

            if (o is not IDisposable)
            {
            }

            if (this is null) // Compliant as it does not check the type.
            {

            }

            if (this is not null) // Compliant as it does not check the type.
            {

            }

            if (this is Program or IDisposable) // Noncompliant
            {
            }

            if (this is Program and IDisposable) // Noncompliant
            {
            }

            if (this is var variable) // Compliant
            {
            }

            if (this is { MyProperty: 5 }) // Compliant as it does not check the type
            {
            }

            if (this is ({ MyProperty: 5 })) // Compliant as it does not check the type
            {
            }

            if (this is not { MyProperty: 5 })
            {
            }

            if (o is null)
            {
            }

            if (this is Program p) // Noncompliant
            {
            }

            if (this is { MyProperty: int prop1 and > 5 }) // Compliant
            {
            }

            if (this is Program { MyProperty: int prop2 and > 5 } e) // Noncompliant
            {
            }

            if (this is { MyProperty2: IDisposable }) // Compliant
            {
            }

            switch (this) // Compliant
            {
                case { MyProperty: 5 }:
                    break;
                case { MyProperty: 3 }:
                case null:
                    break;
                case not null:
                    break;
            }

            switch (this) // Noncompliant
            {
                case IDisposable:
                    break;
                default:
                    break;
            }

            switch (this) // Noncompliant
            {
                case IEnumerable enumerable:
                    break;
                default:
                    break;
            }

            switch (this) // Noncompliant
            {
                case Program { MyProperty: 5 }:
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

            result = this switch // Noncompliant
            {
                IDisposable disposable => 1,
                _ => 3
            };

            result = this switch // Noncompliant
            {
                Program { MyProperty: 5 } => 1,
                _ => 3
            };

            result = this switch // Compliant
            {
                { MyProperty: 5 } => 1,
                { MyProperty: 3 } => 2,
                null => 4,
                not null => 5
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
