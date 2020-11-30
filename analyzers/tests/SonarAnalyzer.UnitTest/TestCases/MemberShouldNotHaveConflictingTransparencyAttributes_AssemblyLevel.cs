using System;
using System.Security;

[assembly: SecurityCritical]
//         ^^^^^^^^^^^^^^^^ Secondary
//         ^^^^^^^^^^^^^^^^ Secondary@-1
//         ^^^^^^^^^^^^^^^^ Secondary@-2
//         ^^^^^^^^^^^^^^^^ Secondary@-3
//         ^^^^^^^^^^^^^^^^ Secondary@-4
//         ^^^^^^^^^^^^^^^^ Secondary@-5
//         ^^^^^^^^^^^^^^^^ Secondary@-6
//         ^^^^^^^^^^^^^^^^ Secondary@-7

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

    public class Program3
    {
        [SecuritySafeCritical] // Noncompliant
        public Program3()
        {
        }
    }

    public class Program4
    {
        [SecuritySafeCritical] // Noncompliant
        public void DoSomething()
        {
        }
    }

    public class Program5
    {
        [SecuritySafeCritical] // Noncompliant
        public class SafeClass
        {
        }
    }

    public class Program6
    {
        [SecuritySafeCritical] // Noncompliant
        public struct SafeStruct
        {
        }
    }

    public class Program7
    {
        [SecuritySafeCritical] // Noncompliant
        public interface ISafe
        {
        }
    }

    public class Program8
    {
        [SecuritySafeCritical] // Noncompliant
        public enum SafeEnum
        {
        }
    }
}
