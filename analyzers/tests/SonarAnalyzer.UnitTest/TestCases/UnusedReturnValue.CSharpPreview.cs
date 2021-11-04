using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class UnusedReturnValue
    {
        private interface If
        {
            static abstract int MyMethodIf();
        }

        private class Nested : If
        {
            public static int MyMethodIf() { return 5; } // Compliant, part of the interface

            public void Test()
            {
                Nested.MyMethodIf();
            }
        }

        private static int MyMethod() { return 42; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
//                     ^^^
        private int MyMethod1() { return 42; } // Compliant, unused, S1144 also reports on it
        private int MyMethod2() { return 42; }
        private int MyMethod3() { return 42; }
        private void MyMethod4() { return; }

        public void Test()
        {
            MyMethod();
            MyMethod4();
            var i = MyMethod2();
            Action<int> a = (x) => MyMethod();
            Func<int> f = () => MyMethod3();
            SomeGenericMethod<object>();

            SomeGenericMethod2<UnusedReturnValue>();
            var o = SomeGenericMethod2<object>();
        }

        private T SomeGenericMethod<T>() where T : class { return null; } // Noncompliant
        private T SomeGenericMethod2<T>() where T : class { return null; }
    }
}
