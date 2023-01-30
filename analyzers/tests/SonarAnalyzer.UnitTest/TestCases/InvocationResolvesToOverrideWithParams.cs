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
        private int InlineInitializedField11 = new SomeClass("1", "s1").PrivateOverload("s1");                  // Compliant
        private int InlineInitializedField12 = new SomeClass("1", "s1").PrivateOverload("s1", "s2");            // Noncompliant
        private int InlineInitializedField13 = new SomeClass("1", "s1").PrivateOverload(null, "s2");            // Noncompliant
        private int InlineInitializedField14 = new SomeClass("1", "s1").PrivateOverload(null, new[] { "s2" });  // Compliant
        private int InlineInitializedField15 = new SomeClass("1", "s1").PrivateOverload("42", "s1", "s2");      // Compliant

        private int InlineInitializedField21 = new SomeClass("1", "s1").InternalOverload("s1");                 // Compliant
        private int InlineInitializedField22 = new SomeClass("1", "s1").InternalOverload("s1", "s2");           // Noncompliant
        private int InlineInitializedField23 = new SomeClass("1", "s1").InternalOverload(null, "s2");           // Noncompliant
        private int InlineInitializedField24 = new SomeClass("1", "s1").InternalOverload(null, new[] { "s2" }); // Compliant
        private int InlineInitializedField25 = new SomeClass("1", "s1").InternalOverload("42", "s1", "s2");     // Compliant

        private SomeClass(object a, string b) { }
        private SomeClass(string a, string b) { }
        public SomeClass(string a, params string[] bs) { }

        private int PrivateOverload(object a, string b) => 1492;
        public int PrivateOverload(string a, params string[] bs) => 1606;

        protected int ProtectedOverload(object a, string b) => 1493;
        public int ProtectedOverload(string a, params string[] bs) => 1607;

        private protected int PrivateProtectedOverload(object a, string b) => 1494;
        public int PrivateProtectedOverload(string a, params string[] bs) => 1608;

        protected internal int ProtectedInternalOverload(object a, string b) => 1495;
        public int ProtectedInternalOverload(string a, params string[] bs) => 1609;

        internal int InternalOverload(object a, string b) => 1496;
        public int InternalOverload(string a, params string[] bs) => 1610;

        protected virtual int OverriddenAsProtected(object a, string b) => 1497;
        public int OverriddenAsProtected(string a, params string[] bs) => 1611;

        protected int ShadowedAsPublic(object a, string b) => 1498;
        public int ShadowedAsPublic(string a, params string[] bs) => 1612;

        protected int ShadowedAsProtectedInternal(object a, string b) => 1499;
        public int ShadowedAsProtectedInternal(string a, params string[] bs) => 1613;

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
                var x = new SomeClass("1", "s1");
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
                    var x = new SomeClass("1", "s1");
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
        private int InlineInitializedField11 = new SomeClass("1", "s1").PrivateOverload("s1");                  // Compliant
        private int InlineInitializedField12 = new SomeClass("1", "s1").PrivateOverload("s1", "s2");            // Compliant
        private int InlineInitializedField13 = new SomeClass("1", "s1").PrivateOverload(null, "s2");            // Compliant
        private int InlineInitializedField14 = new SomeClass("1", "s1").PrivateOverload(null, new[] { "s2" });  // Compliant
        private int InlineInitializedField15 = new SomeClass("1", "s1").PrivateOverload("42", "s1", "s2");      // Compliant

        private int InlineInitializedField21 = new SomeClass("1", "s1").InternalOverload("s1");                 // Compliant
        private int InlineInitializedField22 = new SomeClass("1", "s1").InternalOverload("s1", "s2");           // Noncompliant
        private int InlineInitializedField23 = new SomeClass("1", "s1").InternalOverload(null, "s2");           // Noncompliant
        private int InlineInitializedField24 = new SomeClass("1", "s1").InternalOverload(null, new[] { "s2" }); // Compliant
        private int InlineInitializedField25 = new SomeClass("1", "s1").InternalOverload("42", "s1", "s2");     // Compliant

        public int this[int i]
        {
            get
            {
                var x = new SomeClass("1", "s1");
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
                var x = new SomeClass("1", "s1");
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
            x = new SomeClass("1");                            // Compliant, can't see non-param overload
            x = new SomeClass("1", "s1");                      // Compliant, can't see non-param overload
            x = new SomeClass(null, "s1");                     // Compliant, can't see non-param overload
            x = new SomeClass("1", "s1", "s2");                // Compliant, can't see non-param overload
            x = new SomeClass(null, "s1", "s2");               // Compliant, can't see non-param overload

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
        public SubClass(string a, params string[] bs) : base(a, bs) { }

        protected override int OverriddenAsProtected(object a, string b) => 2494;

        public new int ShadowedAsPublic(object a, string b) => 2498;

        protected internal new int ShadowedAsProtectedInternal(object a, string b) => 2499;

        public void OverridenAndShadowedAccessibility()
        {
            var x = new SubClass("3");
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
                var x = new SomeClass("1", "s1");
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
