using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class UnusedReturnValue
    {
        private interface If
        {
            int MyMethodIf();
        }

        private class Nested : If
        {
            int If.MyMethodIf() { return 5; } // Compliant, part of the interface

            public void Test()
            {
                ((If)this).MyMethodIf();
            }
        }

        private int MyMethod() { return 42; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
//              ^^^
        private int MyMethod1() { return 42; } // Compliant, unused, S1144 also reports on it
        private int MyMethod2() { return 42; }
        private int MyMethod3() { return 42; }

        public void Test()
        {
            MyMethod();
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

    public struct UnusedReturnValueInStruct
    {
        private int MyMethod() { return 42; } // Noncompliant
        public void Test()
        {
            MyMethod();
        }
    }

    public class WithLocalFunctions
    {
        int MyProperty
        {
            get
            {
                GetNumber();
                return 42;

                int GetNumber() { return 42; } // Noncompliant
            }
        }

        public void Method(IEnumerable<string> myEnumerable)
        {
            GetNumber1();
            var result1 = GetNumber3();

            GetNumberStatic1();
            var result2 = GetNumberStatic3();

            GetNumberStaticExpression();

            myEnumerable.Select(GetNumber4);

            int GetNumber1() { return 42; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
//          ^^^
            int GetNumber2() { return 42; } // Compliant - unused local functions are outside the scope of this rule
            int GetNumber3() { return 42; }
            int GetNumber4(string myParam) { return 42; }

            static int GetNumberStatic1() { return 42; } // Noncompliant
            static int GetNumberStatic2() { return 42; } // Compliant -  local functions are outside the scope of this rule
            static int GetNumberStatic3() { return 42; }

            static int GetNumberStaticExpression() => 42; // Noncompliant
        }
    }
}
