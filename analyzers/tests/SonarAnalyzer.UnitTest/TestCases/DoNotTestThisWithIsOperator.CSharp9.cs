using System;
using System.Collections;

namespace Tests.Diagnostics
{
    class Program
    {
        public int MyProperty { get; set; }
        public object MyProperty2 { get; set; }

        public Program(object o, object i)
        {
            if (this is Program and { MyProperty: 2 }) // Noncompliant
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
            }

            if (this is not IDisposable) // Noncompliant {{Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^
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

            if (this is not null and (IDisposable or Program))  // Noncompliant
            {
            }

            if (o is not null and (IDisposable or Program))
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

            if (this is { MyProperty: 5 })
            {
                if (this is Program) // Noncompliant
                {
                }
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

            switch (this) // Noncompliant Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.
//                  ^^^^
            {
                case IDisposable: // Secondary
//                   ^^^^^^^^^^^
                    break;
                default:
                    break;
            }

            switch (this) // Noncompliant
            {
                case IEnumerable enumerable: // Secondary
//                   ^^^^^^^^^^^^^^^^^^^^^^
                    break;
                default:
                    break;
            }

            switch (this) // Noncompliant
            {
                case Program { MyProperty: 5 }: // Secondary
//                   ^^^^^^^^^^^^^^^^^^^^^^^^^
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

            var result = (((this))) switch // Noncompliant Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.
//                       ^^^^^^^^^^
            {
                IDisposable => 1, // Secondary
//              ^^^^^^^^^^^
                IEnumerable => 2, // Secondary
//              ^^^^^^^^^^^
                _ => 3
            };

            result = this switch // Noncompliant
//                   ^^^^
            {
                IDisposable disposable => 1, // Secondary
//              ^^^^^^^^^^^^^^^^^^^^^^
                _ => 3
            };

            result = this switch // Noncompliant
            {
                Program { MyProperty: 5 } => 1, // Secondary
//              ^^^^^^^^^^^^^^^^^^^^^^^^^
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

            if ((o, (this, i)) is (Program, (Program, Program))) // FN
            {
            }

            switch ((this, o, i)) // FN
            {
                case (Program, Program, Program):
                    break;
                default:
                    break;
            }
        }

        public bool SomeMethod(object o)
        {
            if (this is { MyProperty: 5 })
            {
                return this is Program { MyProperty: 5 }; // Noncompliant
            }

            switch (this)
            {
                case { MyProperty: 5 }:
                    return this is Program { MyProperty: 5 }; // Noncompliant
                default:
                    break;
            }

            switch (o)
            {
                case IDisposable:
                    {
                        return this is Program { MyProperty: 5 }; // Noncompliant
                    }
                case IEnumerable:
                    break;
                default:
                    break;
            }

            var result = this switch
            {
                { MyProperty: 5 } => this is Program { MyProperty: 5 }, // Noncompliant
//                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                { MyProperty: 3 } => this is Program { MyProperty: 3 }, // Noncompliant
                _ => false
            };

            return false;
        }
    }

    record R
    {
        void Foo()
        {
            if (this is IDisposable) // Noncompliant {{Offload the code that's conditional on this type test to the appropriate subclass and remove the condition.}}
//              ^^^^^^^^^^^^^^^^^^^
            {
            }
        }
    }
}
