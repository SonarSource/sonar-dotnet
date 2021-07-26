using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Test interface inheritance
namespace Test_101
{
    public interface A
    {
        void Method();
    }

    public class B : A
    {
        public void Method() { }

        public void Test_101(B foo) // Noncompliant {{Consider using more general type 'Test_101.A' instead of 'Test_101.B'.}}
//                             ^^^
        {
            foo.Method();
        }
    }
}

// Test interface inheritance with in-between interface
namespace Test_102
{
    public interface A
    {
        void Method();
    }

    public interface B : A
    {
    }

    public class C : B
    {
        public void Method() { }

        public void Test_102(C foo) // Noncompliant {{Consider using more general type 'Test_102.A' instead of 'Test_102.C'.}}
        {
            foo.Method();
        }
    }
}

// Test interface inheritance with hierarchy
namespace Test_103
{
    public interface A_Base
    {
        void Method();
    }

    public interface A_Derived : A_Base
    {
    }

    public interface B_Base
    {
        void OtherMethod();
    }

    public interface B_Derived : B_Base
    {
    }

    public class C : B_Derived, A_Derived
    {
        public void Method() { }
        public void OtherMethod() { }

        public void Test_103(C foo) // Noncompliant {{Consider using more general type 'Test_103.A_Base' instead of 'Test_103.C'.}}
        {
            foo.Method();
        }
    }
}

// Test new method (without "new" keyword)
namespace Test_104
{
    public class A
    {
        public void Method() { }
    }

    public class B : A
    {
        public void Method() { }

        public void Test_104(B foo)
        {
            foo.Method();
        }
    }
}

// Test new method (with "new" keyword)
namespace Test_105
{
    public class A
    {
        public void Method() { }
    }

    public class B : A
    {
        public new void Method() { }

        public void Test_105(B foo)
        {
            foo.Method();
        }
    }
}

// Test virtual method
namespace Test_106
{
    public class A
    {
        public virtual void Method() { }
    }

    public class B : A
    {
        public override void Method() { }

        public void Test_106(B foo) // Noncompliant {{Consider using more general type 'Test_106.A' instead of 'Test_106.B'.}}
        {
            foo.Method();
        }
    }
}

// Test property inheritance
namespace Test_107
{
    public class A
    {
        public int Property { get; set; }
    }

    public class B : A
    {
        public void Test_107(B foo) // Noncompliant {{Consider using more general type 'Test_107.A' instead of 'Test_107.B'.}}
        {
            int x = foo.Property;
        }
    }
}

// Test property with no base setter
namespace Test_108
{
    public interface A
    {
        int Property { get; }
    }

    public class B : A
    {
        public int Property { get; set; }

        public void Test_108(B foo)
        {
            foo.Property = 1;
        }
    }
}

// Test constraints from other methods
namespace Test_109
{
    public interface A
    {
        int Property { get; }
    }

    public class B : A
    {
        public int Property { get; set; }

        public void Test_109(B foo) // Noncompliant
        {
            MethodA(foo);
        }

        public void Test_109_01(B foo)
        {
            MethodB(foo);
        }

        public void MethodA(A something) { }

        public void MethodB(B something) { }
    }
}

// Test assignments
namespace Test_110
{
    public interface A
    {
        int Property { get; set; }
    }

    public class B : A
    {
        public int Property { get; set; }

        public void Test_110(B foo)
        {
            foo = new Test_110.B();
            var y = foo.Property;
        }

        public void Test_110_01(B foo)
        {
            var x = foo;
            var y = foo.Property;
        }

        public void Test_110_02(B foo) // Noncompliant
        {
            var y = foo.Property;
        }
    }
}

// Test fields
namespace Test_111
{
    public class A
    {
        public int publicField;
        public int publicField_2;

        protected int protectedField;
        protected int protectedField_2;
    }

    public class B : A
    {
        public int publicField;

        new protected int protectedField;

        public void Test_111(B foo)
        {
            foo.publicField = 1;
        }

        public void Test_111_01(B foo) // Noncompliant
        {
            foo.publicField_2 = 1;
        }

        public void Test_111_02(B foo)
        {
            var x = foo.publicField;
        }

        public void Test_111_03(B foo) // Noncompliant
        {
            var x = foo.publicField_2;
        }
    }
}

// Test conditional access
namespace Test_112
{
    public class A
    {
        public int Field;

        public void Method() { }

        public int Property { get; set; }

        public event EventHandler Event
        {
            add { }
            remove { }
        }
    }

    public class B : A
    {
        public void Test_112(B foo) // Noncompliant
        {
            var x = foo?.Field;
        }

        public void Test_112_01(B foo) // Noncompliant
        {
            var x = foo?.Property;
        }

        public void Test_112_02(B foo) // Noncompliant
        {
            foo?.Method();
        }

        public void Test_112_03(B foo) // Noncompliant
        {
            foo.Event += Foo_SomeEvent;
        }

        private void Foo_SomeEvent(object sender, EventArgs e) { }
    }
}

// Test parameter ordering
namespace Test_113
{
    public class A
    {
        public int Field;

        public void Method() { }

        public int Property { get; set; }
    }

    public class B : A
    {
        public void Test_113(Test_113.B foo) // Noncompliant
        {
            MethodA(1, 1, foo, false);
        }
        public void MethodA(int unused, int param2, Test_113.A something, bool f) { }

        public void MethodB(Test_113.B something) { }
    }
}

// Test extension methods
namespace Test_114
{
    public class A
    {
    }

    public class B : A
    {
        public void Test_114(B foo) // Noncompliant
        {
            foo.ExtMethodA();
        }

        public void Test_114_01(B foo)
        {
            foo.ExtMethodB();
        }

        public void Test_114_02(B foo)
        {
            foo?.ExtMethodB();
        }
    }

    public static class Ext
    {
        public static void ExtMethodA(this A foo)
        {
        }

        public static void ExtMethodB(this B foo)
        {
        }
    }
}

// Test events
namespace Test_115
{
    public interface A
    {
        event EventHandler SomeEvent;
    }

    public class B : A
    {
        public event EventHandler SomeEvent
        {
            add { }
            remove { }
        }

        public void Test_115(B foo) // Noncompliant
        {
            foo.SomeEvent += Foo_SomeEvent;
        }

        public void Test_115_01(A foo)
        {
            foo.SomeEvent += Foo_SomeEvent;
        }

        private void Foo_SomeEvent(object sender, EventArgs e) { }
    }
}

// Test multiple parameters
namespace Test_116
{
    public interface A
    {
        void Method();

        int Property { get; }
    }

    public class B : A
    {
        public int Property { get { return 0; } }

        public void Method() { }

        public void OtherMethod() { }

        public void Test_116(B foo1, B foo2, B foo3)
//                             ^^^^ Noncompliant {{Consider using more general type 'Test_116.A' instead of 'Test_116.B'.}}
//                                     ^^^^ Noncompliant@-1 {{Consider using more general type 'Test_116.A' instead of 'Test_116.B'.}}
        {
            foo1.Method();
            var x = foo2.Property;
            foo3.OtherMethod();
        }
    }
}

// Test overridden method
namespace Test_117
{
    public interface IA
    {
        void Method();

        int Property { get; }
    }

    public class A : IA
    {
        public int Property { get { return 0; } }

        public void Method() { }

        // Compliant - method is virtual so there is a contract to respect
        public virtual void Test_117(B foo)
        {
            foo.Method();
        }
    }

    public class B : A
    {
        public int Property { get { return 0; } }

        public void Method() { }

        public void OtherMethod() { }

        // Compliant - cannot change parameter type, because it is an override.
        public override void Test_117(B foo)
        {
            foo.Method();
        }
    }
}

// Test excluded types
namespace Test_118
{
    using System.Collections.Generic;

    public class A
    {
        public enum AreWeGood { Yes, OfCourse }

        // Compliant examples. Do not report on if the suggestion is one of:
        // object
        // string
        // value type
        // array
        // enum
        // types starting with "_"
        // unused parameter

        public void Test_118_Object(B foo)
        {
            var x = foo.ToString();
        }

        public void Test_118_Enum(AreWeGood foo)
        {
            foo.HasFlag(AreWeGood.Yes);
        }

        public void Test_118_Interop(System.Type foo)
        {
            var x = foo.IsEnum;
        }

        public void Test_118_Unused(B foo)
        {
        }

        public void Test_118_Array(B[] foo)
        {
            var x = foo.Length;
        }

        public void Test_118_ValueType(Guid foo)
        {
            var x = foo.ToString();
        }

        public void Test_118_String(string foo)
        {
            var x = foo.Equals("");
        }
    }

    public class B : A { }
}

// Test parentheses
namespace Test_119
{
    public class A
    {
        public void Method() { }

        public int Property { get; set; }
    }

    public class B : A
    {
        public void Test_119_Method(B foo) // Noncompliant
        {
            (((foo))).Method();
        }

        public void Test_119_Method2(B foo) // Noncompliant
        {
            Foo( (((foo))) );
        }

        public void Test_119_Property(B foo) // Noncompliant
        {
            (foo).Property = 1;
        }

        private void Foo(A thing) { }
    }
}

// Test unsupported
namespace Test_120
{
    public class A
    {
        public void Method() { }
    }

    public class B : A
    {
        public void Test_119_Method(B foo) // False negative
        {
            (foo ?? null).Method();
        }
    }
}

// Test implementing interface
namespace Test_121
{
    public interface I
    {
        void Foo();

        void Foo2();
    }

    public class A : I
    {
        public virtual void Foo() { }
        public void Foo2() { }
    }

    public class B : A
    {
        public override void Foo() { }
        public void Foo2() { }
    }

    public interface IOther
    {
        void MethodA(A a);
        void MethodB(B b);
    }

    public class Other : IOther
    {
        public void MethodA(A a)
        {
            a.Foo();
        }

        public void MethodB(B b)
        {
            b.Foo();
        }
    }
}

// Test collection accessed through indexer
namespace Test_123
{
    using System.Collections;
    using System.Collections.Generic;

    public class Foo
    {
        public void MethodOne(IList list)
        {
            if (list.Count > 0)
            {
                Console.WriteLine(list[0]);
            }
        }

        public void MethodTwo(IList<Foo> list)
        {
            if (list.Count > 0)
            {
                Console.WriteLine(list[0]);
            }
        }

        public void MethodThree(IReadOnlyList<Foo> list)
        {
            if (list.Count > 0)
            {
                Console.WriteLine(list[0]);
            }
        }

        public void MethodThree(IList<Foo> list)
        {
            list[0] = new Test_123.Foo();
        }
    }
}

// Test that rule doesn't suggest other types for EventHandler methods
namespace Test_124
{
    public interface IFooEventArgs
    {
        bool IsFoo { get; }
    }
    public class FooEventArgs : EventArgs, IFooEventArgs
    {
        public bool IsFoo { get; set; }
    }

    public class Foo
    {
        public event EventHandler<FooEventArgs> FooEvent;

        public Foo()
        {
            FooEvent += Foo_FooEvent;
        }

        private void Foo_FooEvent(object sender, FooEventArgs e)
        {
            var x = e.IsFoo;
        }
    }
}

namespace Something2
{
    internal interface IFoo
    {
        bool IsFoo { get; }
    }

    public class Foo : IFoo
    {
        public bool IsFoo { get; set; }
    }

    public class Bar : Foo
    {
    }
}

// Test that rule doesn't suggest base with inconsistent accessibility
namespace Test_125
{
    public class Bar
    {
        public void MethodOne(Something2.Foo f)
        {
            var x = f.IsFoo;
        }

        protected void MethodTwo(Something2.Foo f)
        {
            var x = f.IsFoo;
        }

        private void MethodThree(Something2.Foo f) // Noncompliant
        {
            var x = f.IsFoo;
        }

        internal void MethodFour(Something2.Foo f) // Noncompliant
        {
            var x = f.IsFoo;
        }

        public void MethodFive(Something2.Bar f) // Noncompliant {{Consider using more general type 'Something2.Foo' instead of 'Something2.Bar'.}}
        {
            var x = f.IsFoo;
        }

        protected void MethodSix(Something2.Bar f) // Noncompliant {{Consider using more general type 'Something2.Foo' instead of 'Something2.Bar'.}}
        {
            var x = f.IsFoo;
        }

        private void MethodSeven(Something2.Bar f) // Noncompliant {{Consider using more general type 'Something2.IFoo' instead of 'Something2.Bar'.}}
        {
            var x = f.IsFoo;
        }

        internal void MethodEight(Something2.Bar f) // Noncompliant {{Consider using more general type 'Something2.IFoo' instead of 'Something2.Bar'.}}
        {
            var x = f.IsFoo;
        }
    }
}

// Do not suggest ICollection<KVP<T1, T2>> instead of Dictionary<T1, T2>
namespace Test_126
{
    public class Foo
    {
        public void Bar(Dictionary<string, string> dictionary)
        {
            var x = dictionary.Count;
        }

        public void Bar(ICollection<KeyValuePair<string, string>> b)
        {
            var x = b.Count;
        }
    }
}

// Test IEnumerable iterated over twice
namespace Test_126
{
    public class IEnumerable_T_Tests
    {
        protected void IEnumerable_Once(IEnumerable<string> foo)
        {
            var y = foo.Where(z => z != null);
        }

        protected void IEnumerable_Twice(List<string> foo)
        {
            var y = foo.Where(z => z != null);
            y = foo.Where(z => z != null);
        }

        protected void IEnumerable_T_Once_With_List(List<string> foo) // Noncompliant
        {
            var y = foo.Where(z => z != null);
        }
    }

    public class IEnumerable_Tests
    {
        protected void IEnumerable_Once(IEnumerable foo)
        {
            var x = foo.GetEnumerator();
        }

        protected void IEnumerable_Twice(IList foo)
        {
            var x = foo.GetEnumerator();
            var y = foo.GetEnumerator();
        }

        protected void IEnumerable_T_Once_With_List(IList foo) // Noncompliant
        {
            var x = foo.GetEnumerator();
        }
    }
}

namespace Test_127
{
    public sealed class ReproIssue2479
    {
        public void SomeMethod(IReadOnlyList<S3242DeconstructibleType> list) // Noncompliant FP #2479, using IReadOnlyCollection gives compile error
        {
            for (var i = 0; i < list.Count; ++i)
            {
                var (key, value) = list[i];
                Console.WriteLine(key + " " + value);
            }
        }
    }

    public class S3242DeconstructibleType
    {
        public void Deconstruct(out object key, out object value)
        {
            key = value = "x";
        }
    }
}

namespace MultiConditionalAccess2
{
    public class BaseItem
    {
        public string this[int i] => null;

        public string BaseMethod() => null;
    }

    public class MainItem : BaseItem
    {
        public string Property => null;
    }

    public class Usage
    {
        public void Basic(MainItem item) // Noncompliant
        {
            var value = item[42];
            var length = item[42].Length;
        }

        public void ConditionalAccess(MainItem item) // False negative, binding syntax is not supported in ConditionalAccessExpressionSyntax
        {
            var value = item?[42];
            var length = item?[42].Length;
        }

        public void DoubleConditionalAccess(MainItem item) // False negative, binding syntax is not supported in ConditionalAccessExpressionSyntax
        {
            var length = item?[42]?.ToString()?.Length;
        }

        public void CompliantDouble(System.ArgumentException ex)
        {
            var length = ex?.ParamName?.Length;
            var message = ex.Message;
        }

        public void CompliantTriple(System.ArgumentException ex)
        {
            var length = ex?.ParamName?.ToString()?.Length;
            var message = ex.Message;
        }

        public void NoncompliantDouble(System.ArgumentException ex) // Noncompliant
        {
            var length = ex?.Message?.Length;
        }

        public void NoncompliantTriple(System.ArgumentException ex) // Noncompliant
        {
            var length = ex?.Message?.ToString()?.Length;
        }

        public void NoncompliantTripleMethod(MainItem ex) // Noncompliant
        {
            var length = ex?.BaseMethod()?.ToString()?.Length;
        }
    }
}
