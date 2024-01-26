using System;
using System.Security;

[assembly: SecurityCritical]
//         ^^^^^^^^^^^^^^^^ Secondary
//         ^^^^^^^^^^^^^^^^ Secondary@-1

namespace Tests.Diagnostics
{
    public class Program1
    {
        [SecuritySafeCritical] // Noncompliant {{Change or remove this attribute to be consistent with its container.}}
//       ^^^^^^^^^^^^^^^^^^^^
        private int myValue;
    }

    public class Program2
    {
        [SecuritySafeCritical] // Noncompliant
        public delegate void SimpleDelegate();
    }

}
