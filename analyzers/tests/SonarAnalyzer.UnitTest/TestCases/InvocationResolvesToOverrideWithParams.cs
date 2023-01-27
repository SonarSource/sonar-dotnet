using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class InvocationResolvesToOverrideWithParams
{
    public static void Test(int foo, params object[] p)
    {
        Console.WriteLine("test1");
    }

    public static void Test(double foo, object p1)
    {
        Console.WriteLine("test2");
    }

    static void Main(string[] args)
    {
        Test(42, null); // Noncompliant {{Review this call, which partially matches an overload without 'params'. The partial match is 'void InvocationResolvesToOverrideWithParams.Test(double foo, object p1)'.}}
//      ^^^^^^^^^^^^^^
    }

    public InvocationResolvesToOverrideWithParams(string a, params object[] b)
    {

    }
    public InvocationResolvesToOverrideWithParams(object a, object b, object c)
    {

    }

    private void Format(string a, params object[] b) { }

    private void Format(object a, object b, object c, object d = null) { }

    private void Format2(string a, params object[] b) { }

    private void Format2(int a, object b, object c) { }

    private void Format3(params int[] a) { }

    private void Format3(IEnumerable<int> a) { }

    private void Format4(params object[] a) { }

    private void Format4(object o, IEnumerable<object> a) { }

    private void m()
    {
        Format("", null, null); // Noncompliant
        Format(new object(), null, null);
        Format("", new object[0]);

        Format2("", null, null); // Compliant

        new InvocationResolvesToOverrideWithParams("", null, null); //Noncompliant
        new InvocationResolvesToOverrideWithParams(new object(), null, null);

        Format3(new int[0]); //Compliant, although it is also an IEnumerable<int>

        Format4(new object(), new int[0]); // Noncompliant

        Format3(null); // Noncompliant, maybe it could be compliant
        string.Concat("aaaa"); // Noncompliant, resolves to params, but there's a single object version too.

        Console.WriteLine("format", 0, 1, "", ""); // Compliant
    }
}

public class MyClass
{
    public void Format(string a, params object[] b) { }
    public void Format() { } // The presence of this method causes the issue

    public void Test()
    {
        Format("", null, null);
    }
}

public class Test
{
    public void MyMethod(params string[] s) { }
    public void MyMethod(Test s) { }

    public Test()
    {
        MyMethod(""); // Compliant
    }
}

public class Test2
{
    public static implicit operator Test2(string s) { return null; }

    public void MyMethod(params string[] s) { }
    public void MyMethod(Test2 s) { }

    public Test2()
    {
        MyMethod(""); // Noncompliant
    }
}

// See https://github.com/SonarSource/sonar-dotnet/issues/2234
public class FuncAndActionCases
{
    static void Main(string[] args)
    {
        M1(() => Console.WriteLine("hi"));
    }

    public static void M1(params Action[] a) { }
    public static void M1<T>(Func<T> f) { }
    public static void M1(Func<Task> f) { }
}

public class WithLocalFunctions
{
    public static void Test(int foo, params object[] p)
    {
        Console.WriteLine("test1");
    }

    public static void Test(double foo, object p1)
    {
        Console.WriteLine("test2");
    }

    public void Method()
    {
        static void Call()
        {
            Test(42, null); // Noncompliant {{Review this call, which partially matches an overload without 'params'. The partial match is 'void WithLocalFunctions.Test(double foo, object p1)'.}}
        }
    }
}

// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/5430
namespace Repro5430
{
    public class SomeClass
    {
        private SomeClass(int a, string b) { }
        public SomeClass(int a, params string[] bs) { }

        private void PrivateOverloadOtherParams(int a, string b) { }
        public void PrivateOverloadOtherParams(int a, params string[] bs) { }

        private void PrivateOverloadOnlyParams() { }
        public void PrivateOverloadOnlyParams(params string[] bs) { }

        protected void ProtectedOverload() { }
        public void ProtectedOverload(params string[] bs) { }

        private protected void PrivateProtectedOverload() { }
        public void PrivateProtectedOverload(params string[] bs) { }

        protected internal void ProtectedInternalOverload() { }
        public void ProtectedInternalOverload(params string[] bs) { }

        internal void InternalOverload() { }
        public void InternalOverload(params string[] bs) { }

        protected virtual void OverriddenAsProtected() { }
        public void OverriddenAsProtected(params string[] bs) { }

        protected void ShadowedAsPublic() { }
        public void ShadowedAsPublic(params string[] bs) { }

        protected void ShadowedAsProtectedInternal() { }
        public void ShadowedAsProtectedInternal(params string[] bs) { }

        public class NestedClass
        {
            public void Basics()
            {
                SomeClass x;
                x = new SomeClass(1);                         // Noncompliant
                x = new SomeClass(1, "s1");                   // Noncompliant
                x = new SomeClass(1, "s1", "s2");             // Noncompliant

                x.PrivateOverloadOtherParams(42);             // Noncompliant
                x.PrivateOverloadOtherParams(42, "s1");       // Noncompliant
                x.PrivateOverloadOtherParams(42, "s1", "s2"); // Noncompliant

                x.PrivateOverloadOnlyParams();                // Noncompliant
                x.PrivateOverloadOnlyParams("s1");            // Noncompliant
                x.PrivateOverloadOnlyParams("s1", "s2");      // Noncompliant

                x.ProtectedOverload();                        // Noncompliant
                x.ProtectedOverload("s1");                    // Noncompliant
                x.ProtectedOverload("s1", "s2");              // Noncompliant

                x.PrivateProtectedOverload();                 // Noncompliant
                x.PrivateProtectedOverload("s1");             // Noncompliant
                x.PrivateProtectedOverload("s1", "s2");       // Noncompliant

                x.ProtectedInternalOverload();                // Noncompliant
                x.ProtectedInternalOverload("s1");            // Noncompliant
                x.ProtectedInternalOverload("s1", "s2");      // Noncompliant

                x.InternalOverload();                         // Noncompliant
                x.InternalOverload("s1");                     // Noncompliant
                x.InternalOverload("s1", "s2");               // Noncompliant
            }
        }
    }

    public class SubClass : SomeClass
    {
        public SubClass(int a, params string[] bs) : base(a, bs) { }

        protected override void OverriddenAsProtected() { }

        public new void ShadowedAsPublic() { }

        protected internal new void ShadowedAsProtectedInternal() { }
    }

    public class OtherClass
    {
        public void Basics()
        {
            SomeClass x;
            x = new SomeClass(2);                         // Compliant
            x = new SomeClass(2, "s1");                   // Compliant
            x = new SomeClass(2, "s1", "s2");             // Compliant

            x.PrivateOverloadOtherParams(42);             // Compliant
            x.PrivateOverloadOtherParams(42, "s1");       // Compliant
            x.PrivateOverloadOtherParams(42, "s1", "s2"); // Compliant

            x.PrivateOverloadOnlyParams();                // Compliant
            x.PrivateOverloadOnlyParams("s1");            // Compliant
            x.PrivateOverloadOnlyParams("s1", "s2");      // Compliant

            x.ProtectedOverload();                        // Compliant
            x.ProtectedOverload("s1");                    // Compliant
            x.ProtectedOverload("s1", "s2");              // Compliant

            x.PrivateProtectedOverload();                 // Compliant
            x.PrivateProtectedOverload("s1");             // Compliant
            x.PrivateProtectedOverload("s1", "s2");       // Compliant

            x.ProtectedInternalOverload();                // Noncompliant
            x.ProtectedInternalOverload("s1");            // Noncompliant
            x.ProtectedInternalOverload("s1", "s2");      // Noncompliant

            x.InternalOverload();                         // Noncompliant
            x.InternalOverload("s1");                     // Noncompliant
            x.InternalOverload("s1", "s2");               // Noncompliant
        }

        public void Overrides()
        {
            var x = new SubClass(3);
            x.OverriddenAsProtected();                 // Compliant
            x.OverriddenAsProtected("s1");             // Compliant
            x.OverriddenAsProtected("s1", "s2");       // Compliant

            x.ShadowedAsPublic();                      // Noncompliant
            x.ShadowedAsPublic("s1");                  // Noncompliant
            x.ShadowedAsPublic("s1", "s2");            // Noncompliant

            x.ShadowedAsProtectedInternal();           // Noncompliant
            x.ShadowedAsProtectedInternal("s1");       // Noncompliant
            x.ShadowedAsProtectedInternal("s1", "s2"); // Noncompliant
        }
    }
}
