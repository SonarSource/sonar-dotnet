using System;
using System.Collections.Generic;
using System.Linq;

interface InterfaceWithDefaultImplementation
{
    int SomeMethod() // Compliant, part of the interface
    {
        return 42;
    }
}

class SomeClass : InterfaceWithDefaultImplementation
{
    public void Test()
    {
        ((InterfaceWithDefaultImplementation)this).SomeMethod();
    }
}

class StaticLocalFunctions
{
    public void Test()
    {
        GetNumberStatic1();
        var result2 = GetNumberStatic3();
        GetNumberStatic4(1);
        GetNumberStaticExpression();
    }

    static int GetNumberStatic1() { return 42; } // Noncompliant
    static int GetNumberStatic2() { return 42; } // Compliant -  local functions are outside the scope of this rule
    static int GetNumberStatic3() { return 42; }
    static int GetNumberStatic4(int neverUsedVal) { return neverUsedVal; } // Noncompliant
    static int GetNumberStaticExpression() => 42; // Noncompliant
}

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

public static class Extensions
{
    public class Sample
    {
        public void Test()
        {
            this.NonCompliant();
            var x = this.Compliant();
        }
    }

    extension(Sample s)
    {
        private int NonCompliant() => 42; // FN https://sonarsource.atlassian.net/browse/NET-2829
        private int Compliant() => 42;
    }
}

public class FieldKeyword
{
    private int Compliant() => 42;
    public int Prop
    {
        get { return field; }
        set { field = Compliant(); }
    }
}

public class NullConditionalAssignment
{
    public class Sample
    {
        public int Value { get; set; }
        private int Compliant() => 42;

        public void Test()
        {
            this?.Value = Compliant();
        }
    }
}
