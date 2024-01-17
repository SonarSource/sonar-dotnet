using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Program
    {
        public List<int> MyProperty { get; set; }

        public Program(object o, object i)
        {
            if (this is Program { MyProperty.Count: 2 }) // Noncompliant
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
            }

            if (o is Program and { MyProperty.Count: 2 })
            {
            }

            switch (this) // Noncompliant
            {
                case Program { MyProperty.Count: 5 }: // Secondary
//                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    break;
                default:
                    break;
            }

            var result = this switch // Noncompliant
            {
                Program { MyProperty.Count: 5 } => 1, // Secondary
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                _ => 3
            };
        }
    }
}
