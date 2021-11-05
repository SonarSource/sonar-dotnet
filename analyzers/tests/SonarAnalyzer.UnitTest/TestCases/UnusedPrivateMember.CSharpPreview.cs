using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    class UnusedPrivateMember
    {
        private interface MyInterface
        {
            static abstract void Method();
        }
        private class Class1 : MyInterface // Noncompliant {{Remove the unused private type 'Class1'.}}
        {
            public static void Method() { var x = 42; }
            public void Method1() { var x = Method2(); } // Noncompliant {{Remove the unused private method 'Method1'.}}
            public static int Method2() { return 2; }
        }
    }
}
