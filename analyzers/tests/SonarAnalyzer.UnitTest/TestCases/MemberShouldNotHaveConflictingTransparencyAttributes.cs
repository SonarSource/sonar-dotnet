using System;
using System.Security;

namespace Tests.Diagnostics
{
    [SecurityCritical] // Secondary
//   ^^^^^^^^^^^^^^^^
    public class Program1
    {
        [SecuritySafeCritical] // Noncompliant {{Change or remove this attribute to be consistent with its container.}}
//       ^^^^^^^^^^^^^^^^^^^^
        private int myValue;
    }

    [SecurityCritical] // Secondary
    public class Program2
    {
        [SecuritySafeCritical] // Noncompliant
        public delegate void SimpleDelegate();
    }

    [SecurityCritical] // Secondary
    public class Program3
    {

        [SecuritySafeCritical] // Noncompliant
        public Program3()
        {
        }
    }

    [SecurityCritical] // Secondary
    public class Program4
    {

        [SecuritySafeCritical] // Noncompliant
        public void DoSomething()
        {
        }
    }

    [SecurityCritical] // Secondary
    public class Program5
    {

        [SecuritySafeCritical] // Noncompliant
        public class SafeClass
        {
        }
    }

    [SecurityCritical] // Secondary
    public class Program6
    {

        [SecuritySafeCritical] // Noncompliant
        public struct SafeStruct
        {
        }
    }

    [SecurityCritical] // Secondary
    public class Program7
    {

        [SecuritySafeCritical] // Noncompliant
        public interface ISafe
        {
        }
    }

    [SecurityCritical] // Secondary
    public class Program8
    {

        [SecuritySafeCritical] // Noncompliant
        public enum SafeEnum
        {
        }
    }
}
