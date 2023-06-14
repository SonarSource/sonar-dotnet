using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private void MyMethod4() { return; }
        private int MyMethod5(int neverUsedVal) => neverUsedVal; // Noncompliant
        private int MyMethod6(int neverUsedVal) // Noncompliant
        {
            return neverUsedVal;
        }

        private async Task MyAsyncMethod() { return; }

        public void Test()
        {
            MyMethod();
            MyMethod4();
            MyMethod5(1);
            MyMethod6(1);
            MyAsyncMethod();
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
            VoidFunction();
            var result1 = GetNumber3();

            GetNumberStatic1();
            var result2 = GetNumberStatic3();

            GetNumberStaticExpression();

            myEnumerable.Select(GetNumber4);

            GetNumber5(1);
            GetNumberStatic4(1);

            int GetNumber1() { return 42; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
//          ^^^
            int GetNumber2() { return 42; } // Compliant - unused local functions are outside the scope of this rule
            int GetNumber3() { return 42; }
            int GetNumber4(string myParam) { return 42; }
            int GetNumber5(int neverUsedVal) { return neverUsedVal; } // Noncompliant

            void VoidFunction() { return; }

            static int GetNumberStatic1() { return 42; } // Noncompliant
            static int GetNumberStatic2() { return 42; } // Compliant -  local functions are outside the scope of this rule
            static int GetNumberStatic3() { return 42; }
            static int GetNumberStatic4(int neverUsedVal) { return neverUsedVal; } // Noncompliant

            static int GetNumberStaticExpression() => 42; // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7429
    public class Repro7429
    {
        void Method(AnInterface foo)
        {
            Func(0);
            foo?.DoSomething(Func);

            bool Func(int value) => true; // Noncompliant, FP
        }

        void Method()
        {
            Func(0);
            Func2(Func);

            bool Func(int value) => true; // Noncompliant, FP
            void Func2(Func<int, bool> method) { }
        }

        public interface AnInterface
        {
            void DoSomething(Func<int, bool> method);
        }
    }
}
