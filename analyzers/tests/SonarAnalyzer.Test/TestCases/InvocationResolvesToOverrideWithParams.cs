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

// https://github.com/SonarSource/sonar-dotnet/issues/5430
namespace Repro5430
{
    public class SomeClass
    {
        private int Field11 = new SomeClass().PrivateOverload("s1");                  // Compliant
        private int Field12 = new SomeClass().PrivateOverload("s1", "s2");            // Noncompliant
        private int Field13 = new SomeClass().PrivateOverload(null, "s2");            // Noncompliant
        private int Field14 = new SomeClass().PrivateOverload(null, new[] { "s2" });  // Compliant
        private int Field15 = new SomeClass().PrivateOverload("42", "s1", "s2");      // Compliant

        private int Field21 = new SomeClass().InternalOverload("s1");                 // Compliant
        private int Field22 = new SomeClass().InternalOverload("s1", "s2");           // Noncompliant
        private int Field23 = new SomeClass().InternalOverload(null, "s2");           // Noncompliant
        private int Field24 = new SomeClass().InternalOverload(null, new[] { "s2" }); // Compliant
        private int Field25 = new SomeClass().InternalOverload("42", "s1", "s2");     // Compliant

        public SomeClass() { }
        private SomeClass(object a, string b) { }
        private SomeClass(string a, string b) { }
        public SomeClass(string a, params string[] bs) { }

        private int PrivateOverload(object a, string b) => 42;
        public int PrivateOverload(string a, params string[] bs) => 42;

        protected int ProtectedOverload(object a, string b) => 42;
        public int ProtectedOverload(string a, params string[] bs) => 42;

        private protected int PrivateProtectedOverload(object a, string b) => 42;
        public int PrivateProtectedOverload(string a, params string[] bs) => 42;

        protected internal int ProtectedInternalOverload(object a, string b) => 42;
        public int ProtectedInternalOverload(string a, params string[] bs) => 42;

        internal int InternalOverload(object a, string b) => 42;
        public int InternalOverload(string a, params string[] bs) => 42;

        protected virtual int OverriddenAsProtected(object a, string b) => 42;
        public int OverriddenAsProtected(string a, params string[] bs) => 42;

        protected int ShadowedAsPublic(object a, string b) => 42;
        public int ShadowedAsPublic(string a, params string[] bs) => 42;

        protected int ShadowedAsProtectedInternal(object a, string b) => 42;
        public int ShadowedAsProtectedInternal(string a, params string[] bs) => 42;

        public void AllOverloadsVisibleFromSameClass()
        {
            SomeClass x;
            x = new SomeClass("1");                          // Compliant, can't resolve to non-param overload
            x = new SomeClass("1", "s1");                    // Compliant, resolves to non-param overload
            x = new SomeClass(null, "s1");                   // Compliant, resolves to non-param overload
            x = new SomeClass("1", "s1", "s2");              // Compliant, can't resolve to non-param overload
            x = new SomeClass(null, "s1", "s2");             // Compliant, can't resolve to non-param overload

            PrivateOverload("42");                           // Compliant
            PrivateOverload("42", "s1");                     // Noncompliant
            PrivateOverload(null, "s1");                     // Noncompliant
            PrivateOverload(null, new[] { "s2" });           // Compliant
            PrivateOverload("42", "s1", "s2");               // Compliant

            ProtectedOverload("s1");                         // Compliant
            ProtectedOverload("s1", "s2");                   // Noncompliant
            ProtectedOverload(null, "s2");                   // Noncompliant
            ProtectedOverload(null, new[] { "s2" });         // Compliant
            ProtectedOverload("42", "s1", "s2");             // Compliant

            PrivateProtectedOverload("s1");                  // Compliant
            PrivateProtectedOverload("s1", "s2");            // Noncompliant
            PrivateProtectedOverload(null, "s2");            // Noncompliant
            PrivateProtectedOverload(null, new[] { "s2" });  // Compliant
            PrivateProtectedOverload("42", "s1", "s2");      // Compliant

            ProtectedInternalOverload("s1");                 // Compliant
            ProtectedInternalOverload("s1", "s2");           // Noncompliant
            ProtectedInternalOverload(null, "s2");           // Noncompliant
            ProtectedInternalOverload(null, new[] { "s2" }); // Compliant
            ProtectedInternalOverload("42", "s1", "s2");     // Compliant

            InternalOverload("s1");                          // Compliant
            InternalOverload("s1", "s2");                    // Noncompliant
            InternalOverload(null, "s2");                    // Noncompliant
            InternalOverload(null, new[] { "s2" });          // Compliant
            InternalOverload("42", "s1", "s2");              // Compliant
        }

        public class NestedClass
        {
            public void AllOverloadsVisibleFromWithinNestedClass()
            {
                var x = new SomeClass();
                x.PrivateOverload("42");                 // Compliant
                x.PrivateOverload("42", "s1");           // Noncompliant
                x.PrivateOverload(null, "s1");           // Noncompliant
                x.PrivateOverload(null, new[] { "s2" }); // Compliant
                x.PrivateOverload("42", "s1", "s2");     // Compliant
            }

            public struct Nested2ndLevel
            {
                public void AllOverloadsVisibleFromWithinNested2ndLevel()
                {
                    var x = new SomeClass();
                    x.PrivateOverload("42");                 // Compliant
                    x.PrivateOverload("42", "s1");           // Noncompliant
                    x.PrivateOverload(null, "s1");           // Noncompliant
                    x.PrivateOverload(null, new[] { "s2" }); // Compliant
                    x.PrivateOverload("42", "s1", "s2");     // Compliant
                }
            }
        }
    }

    public class OtherClass
    {
        private int Field11 = new SomeClass().PrivateOverload("s1");                  // Compliant
        private int Field12 = new SomeClass().PrivateOverload("s1", "s2");            // Compliant
        private int Field13 = new SomeClass().PrivateOverload(null, "s2");            // Compliant
        private int Field14 = new SomeClass().PrivateOverload(null, new[] { "s2" });  // Compliant
        private int Field15 = new SomeClass().PrivateOverload("42", "s1", "s2");      // Compliant

        private int Field21 = new SomeClass().InternalOverload("s1");                 // Compliant
        private int Field22 = new SomeClass().InternalOverload("s1", "s2");           // Noncompliant
        private int Field23 = new SomeClass().InternalOverload(null, "s2");           // Noncompliant
        private int Field24 = new SomeClass().InternalOverload(null, new[] { "s2" }); // Compliant
        private int Field25 = new SomeClass().InternalOverload("42", "s1", "s2");     // Compliant

        public int this[int i]
        {
            get
            {
                var x = new SomeClass();
                x.InternalOverload("s1");                      // Compliant
                x.InternalOverload("s1", "s2");                // Noncompliant
                x.InternalOverload(null, "s2");                // Noncompliant
                x.InternalOverload(null, new[] { "s2" });      // Compliant
                x.InternalOverload("42", "s1", "s2");          // Compliant
                return 42;
            }
        }

        public int CheckAccessibilityInProperty
        {
            get
            {
                var x = new SomeClass();
                x.InternalOverload("s1");                      // Compliant
                x.InternalOverload("s1", "s2");                // Noncompliant
                x.InternalOverload(null, "s2");                // Noncompliant
                x.InternalOverload(null, new[] { "s2" });      // Compliant
                x.InternalOverload("42", "s1", "s2");          // Compliant
                return 42;
            }
        }

        public void CheckAccessibilityInMethod()
        {
            SomeClass x;
            x = new SomeClass("1");                            // Compliant, can't see non-param overload because it's private
            x = new SomeClass("1", "s1");                      // Compliant
            x = new SomeClass(null, "s1");                     // Compliant
            x = new SomeClass("1", "s1", "s2");                // Compliant
            x = new SomeClass(null, "s1", "s2");               // Compliant

            x.PrivateOverload("42");                           // Compliant
            x.PrivateOverload("42", "s1");                     // Compliant
            x.PrivateOverload(null, "s1");                     // Compliant
            x.PrivateOverload(null, new[] { "s2" });           // Compliant
            x.PrivateOverload("42", "s1", "s2");               // Compliant

            x.ProtectedOverload("s1");                         // Compliant
            x.ProtectedOverload("s1", "s2");                   // Compliant
            x.ProtectedOverload(null, "s2");                   // Compliant
            x.ProtectedOverload(null, new[] { "s2" });         // Compliant
            x.ProtectedOverload("42", "s1", "s2");             // Compliant

            x.PrivateProtectedOverload("s1");                  // Compliant
            x.PrivateProtectedOverload("s1", "s2");            // Compliant
            x.PrivateProtectedOverload(null, "s2");            // Compliant
            x.PrivateProtectedOverload(null, new[] { "s2" });  // Compliant
            x.PrivateProtectedOverload("42", "s1", "s2");      // Compliant

            x.ProtectedInternalOverload("s1");                 // Compliant
            x.ProtectedInternalOverload("s1", "s2");           // Noncompliant
            x.ProtectedInternalOverload(null, "s2");           // Noncompliant
            x.ProtectedInternalOverload(null, new[] { "s2" }); // Compliant
            x.ProtectedInternalOverload("42", "s1", "s2");     // Compliant

            x.InternalOverload("s1");                          // Compliant
            x.InternalOverload("s1", "s2");                    // Noncompliant
            x.InternalOverload(null, "s2");                    // Noncompliant
            x.InternalOverload(null, new[] { "s2" });          // Compliant
            x.InternalOverload("42", "s1", "s2");              // Compliant
        }
    }

    public class SubClass : SomeClass
    {
        public SubClass(): base() { }
        public SubClass(string a, params string[] bs) : base(a, bs) { }

        protected override int OverriddenAsProtected(object a, string b) => 42;

        public new int ShadowedAsPublic(object a, string b) => 42;

        protected internal new int ShadowedAsProtectedInternal(object a, string b) => 42;

        public void OverridenAndShadowedAccessibility()
        {
            var x = new SubClass();
            x.OverriddenAsProtected("42");                       // Compliant
            x.OverriddenAsProtected("42", "s1");                 // Noncompliant, overrides doesn't change priority
            x.OverriddenAsProtected(null, "s1");                 // Noncompliant, overrides doesn't change priority
            x.OverriddenAsProtected(null, new[] { "s2" });       // Compliant
            x.OverriddenAsProtected("42", "s1", "s2");           // Compliant

            x.ShadowedAsPublic("s1");                            // Compliant
            x.ShadowedAsPublic("s1", "s2");                      // Compliant, shadowing method takes priority
            x.ShadowedAsPublic(null, "s1");                      // Compliant, shadowing method takes priority
            x.ShadowedAsPublic(null, new[] { "s2" });            // Compliant
            x.ShadowedAsPublic("42", "s1", "s2");                // Compliant

            x.ShadowedAsProtectedInternal("s1");                 // Compliant
            x.ShadowedAsProtectedInternal("s1", "s2");           // Compliant, shadowing method takes priority
            x.ShadowedAsProtectedInternal(null, "s1");           // Compliant, shadowing method takes priority
            x.ShadowedAsProtectedInternal(null, new[] { "s2" }); // Compliant
            x.ShadowedAsProtectedInternal("42", "s1", "s2");     // Compliant
        }

        public class NestedClass
        {
            public void AllOverloadsVisibleFromWithinNestedClass()
            {
                var x = new SomeClass();
                x.PrivateOverload("42");                 // Compliant
                x.PrivateOverload("42", "s1");           // Compliant
                x.PrivateOverload(null, "s1");           // Compliant
                x.PrivateOverload(null, new[] { "s2" }); // Compliant
                x.PrivateOverload("42", "s1", "s2");     // Compliant
            }
        }
    }

    public class SubClassOfClassFromAnotherAssembly : FromAnotherAssembly
    {
        public void AccessibilityAcrossAssemblies()
        {
            ProtectedOverload("42");                         // Compliant
            ProtectedOverload("42", "s1");                   // Noncompliant, protected visible across assemblies
            ProtectedOverload(null, "s1");                   // Noncompliant, protected visible across assemblies
            ProtectedOverload(null, new[] { "s2" });         // Compliant
            ProtectedOverload("42", "s1", "s2");             // Compliant

            PrivateProtectedOverload("42");                  // Compliant
            PrivateProtectedOverload("42", "s1");            // Compliant, private protected not visible across assemblies
            PrivateProtectedOverload(null, "s1");            // Compliant, private protected not visible across assemblies
            PrivateProtectedOverload(null, new[] { "s2" });  // Compliant
            PrivateProtectedOverload("42", "s1", "s2");      // Compliant

            ProtectedInternalOverload("42");                 // Compliant
            ProtectedInternalOverload("42", "s1");           // Noncompliant, protected internal visible across assemblies
            ProtectedInternalOverload(null, "s1");           // Noncompliant, protected internal visible across assemblies
            ProtectedInternalOverload(null, new[] { "s2" }); // Compliant
            ProtectedInternalOverload("42", "s1", "s2");     // Compliant

            InternalOverload("42");                          // Compliant
            InternalOverload("42", "s1");                    // Compliant, internal not visible across assemblies
            InternalOverload(null, "s1");                    // Compliant, internal not visible across assemblies
            InternalOverload(null, new[] { "s2" });          // Compliant
            InternalOverload("42", "s1", "s2");              // Compliant
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8522
class Repro_8522
{
    T Get<T>(params string[] key) => default;
    string Get(string key) => default;

    T GetBothHaveGenerics<T>(params int[] ints) => default;
    T GetBothHaveGenerics<T>(int anInt) => default;

    T GenericsWhereOneHasObjectParam<T>(params int[] ints) => default;
    T GenericsWhereOneHasObjectParam<T>(object anInt) => default;

    void Test()
    {
        Get<string>("text");                          // Compliant
        GetBothHaveGenerics<string>(1);               // Compliant, when both methods are generic it seems to resolve correctly to the T GetBothHaveGenerics<T>(int anInt).
        GenericsWhereOneHasObjectParam<string>(1);    // Noncompliant
    }
}
