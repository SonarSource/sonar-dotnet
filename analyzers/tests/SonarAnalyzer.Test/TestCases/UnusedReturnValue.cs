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

            myEnumerable.Select(GetNumber4);

            GetNumber5(1);

            int GetNumber1() { return 42; } // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
//          ^^^
            int GetNumber2() { return 42; } // Compliant - unused local functions are outside the scope of this rule
            int GetNumber3() { return 42; }
            int GetNumber4(string myParam) { return 42; }
            int GetNumber5(int neverUsedVal) { return neverUsedVal; } // Noncompliant

            void VoidFunction() { return; }
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

public partial class Partial
{
    private int GetValue1()
    {
        return 1;
    }

    private int GetValue2() // Noncompliant
    {
        return 1;
    }
}
public class CallbackUser
{
    public CallbackUser(Func<int, int> getValue) { }
}

// https://sonarsource.atlassian.net/browse/NET-2829 - private (old-syntax) extension methods
public static class PrivateStaticAndExtensions
{
    public class Sample
    {
        public void Test()
        {
            this.ExtensionNonCompliant();                             // Return value discarded via extension-call syntax.
            PrivateStaticAndExtensions.StaticNonCompliant(this);      // Return value discarded via static-call syntax.
            var x = this.ExtensionCompliant();                        // Return value used via extension-call syntax.
            var y = PrivateStaticAndExtensions.StaticCompliant(this); // Return value used via static-call syntax.
            this.FluentExtension();                                   // Fluent extension, return value discarded.
            this.NonFluentExtension();                                // Non-fluent extension, return value discarded.
        }
    }

    private static int ExtensionNonCompliant(this Sample sample) => 42; // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
//                 ^^^
    private static int ExtensionCompliant(this Sample sample) => 42;

    private static int StaticNonCompliant(Sample sample) => 42; // Noncompliant{{Change return type to 'void'; not a single caller uses the returned value.}}
//                 ^^^
    private static int StaticCompliant(Sample sample) => 42;

    private static Sample FluentExtension(this Sample sample) => sample; // Compliant, fluent pattern (returns this type)

    private static int NonFluentExtension(this Sample sample) => 42; // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
//                 ^^^
}

// https://github.com/SonarSource/sonar-dotnet/issues/9813 - fluent extension chains where only the last method is flagged
public static class FluentChain
{
    public class Container
    {
        public void Test()
        {
            var builder = new List<int>();
            builder
                .AddFoo()
                .AddBar();               // Last in chain, return value discarded.
            Use(builder);                // Original variable still used after the chain.
            NonFluent(builder);          // Non-fluent, return value discarded.
        }

        private static void Use(List<int> list) { }
    }

    private static List<int> AddFoo(this List<int> list)  // Compliant, return value used by AddBar
    {
        list.Add(1);
        return list;
    }

    private static List<int> AddBar(this List<int> list) => list; // Compliant, fluent pattern (returns this type)

    private static int NonFluent(this List<int> list) => 42; // Noncompliant {{Change return type to 'void'; not a single caller uses the returned value.}}
//                 ^^^
}
