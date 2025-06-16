using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Test interface inheritance
namespace AppendedNamespaceForConcurrencyTest.Test_01
{
    public interface A
    {
        void Method();
    }

    public class B : A
    {
        public void Method() { }

        public void Test_01(B foo) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Test_01.A' instead of 'AppendedNamespaceForConcurrencyTest.Test_01.B'.}}
//                            ^^^
        {
            foo.Method();
        }
    }
}

// Test interface inheritance with in-between interface
namespace AppendedNamespaceForConcurrencyTest.Test_02
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

        public void Test_02(C foo) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Test_02.A' instead of 'AppendedNamespaceForConcurrencyTest.Test_02.C'.}}
        {
            foo.Method();
        }
    }
}

// Test interface inheritance with hierarchy
namespace AppendedNamespaceForConcurrencyTest.Test_03
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

        public void Test_03(C foo) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Test_03.A_Base' instead of 'AppendedNamespaceForConcurrencyTest.Test_03.C'.}}
        {
            foo.Method();
        }
    }
}

// Test new method (without "new" keyword)
namespace AppendedNamespaceForConcurrencyTest.Test_04
{
    public class A
    {
        public void Method() { }
    }

    public class B : A
    {
        public void Method() { }

        public void Test_04(B foo)
        {
            foo.Method();
        }
    }
}

// Test new method (with "new" keyword)
namespace AppendedNamespaceForConcurrencyTest.Test_05
{
    public class A
    {
        public void Method() { }
    }

    public class B : A
    {
        public new void Method() { }

        public void Test_05(B foo)
        {
            foo.Method();
        }
    }
}

// Test virtual method
namespace AppendedNamespaceForConcurrencyTest.Test_06
{
    public class A
    {
        public virtual void Method() { }
    }

    public class B : A
    {
        public override void Method() { }

        public void Test_06(B foo) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Test_06.A' instead of 'AppendedNamespaceForConcurrencyTest.Test_06.B'.}}
        {
            foo.Method();
        }
    }
}

// Test property inheritance
namespace AppendedNamespaceForConcurrencyTest.Test_07
{
    public class A
    {
        public int Property { get; set; }
    }

    public class B : A
    {
        public void Test_07(B foo) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Test_07.A' instead of 'AppendedNamespaceForConcurrencyTest.Test_07.B'.}}
        {
            int x = foo.Property;
        }
    }
}

// Test property with no base setter
namespace AppendedNamespaceForConcurrencyTest.Test_08
{
    public interface A
    {
        int Property { get; }
    }

    public class B : A
    {
        public int Property { get; set; }

        public void Test_08(B foo)
        {
            foo.Property = 1;
        }
    }
}

// Test constraints from other methods
namespace AppendedNamespaceForConcurrencyTest.Test_09
{
    public interface A
    {
        int Property { get; }
    }

    public class B : A
    {
        public int Property { get; set; }

        public void Test_09(B foo) // Noncompliant
        {
            MethodA(foo);
        }

        public void Test_09_01(B foo)
        {
            MethodB(foo);
        }

        public void MethodA(A something) { }

        public void MethodB(B something) { }
    }
}

// Test assignments
namespace AppendedNamespaceForConcurrencyTest.Test_10
{
    public interface A
    {
        int Property { get; set; }
    }

    public class B : A
    {
        public int Property { get; set; }

        public void Test_10(B foo)
        {
            foo = new Test_10.B();
            var y = foo.Property;
        }

        public void Test_10_01(B foo)
        {
            var x = foo;
            var y = foo.Property;
        }

        public void Test_10_02(B foo) // Noncompliant
        {
            var y = foo.Property;
        }
    }
}

// Test fields
namespace AppendedNamespaceForConcurrencyTest.Test_11
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

        public void Test_11(B foo)
        {
            foo.publicField = 1;
        }

        public void Test_11_01(B foo) // Noncompliant
        {
            foo.publicField_2 = 1;
        }

        public void Test_11_02(B foo)
        {
            var x = foo.publicField;
        }

        public void Test_11_03(B foo) // Noncompliant
        {
            var x = foo.publicField_2;
        }
    }
}

// Test conditional access
namespace AppendedNamespaceForConcurrencyTest.Test_12
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
        public void Test_12(B foo) // Noncompliant
        {
            var x = foo?.Field;
        }

        public void Test_12_01(B foo) // Noncompliant
        {
            var x = foo?.Property;
        }

        public void Test_12_02(B foo) // Noncompliant
        {
            foo?.Method();
        }

        public void Test_12_03(B foo) // Noncompliant
        {
            foo.Event += Foo_SomeEvent;
        }

        private void Foo_SomeEvent(object sender, EventArgs e) { }
    }
}

// Test parameter ordering
namespace AppendedNamespaceForConcurrencyTest.Test_13
{
    public class A
    {
        public int Field;

        public void Method() { }

        public int Property { get; set; }
    }

    public class B : A
    {
        public void Test_13(Test_13.B foo) // Noncompliant
        {
            MethodA(1, 1, foo, false);
        }
        public void MethodA(int unused, int param2, Test_13.A something, bool f) { }

        public void MethodB(Test_13.B something) { }
    }
}

// Test extension methods
namespace AppendedNamespaceForConcurrencyTest.Test_14
{
    public class A
    {
    }

    public class B : A
    {
        public void Test_14(B foo) // Noncompliant
        {
            foo.ExtMethodA();
        }

        public void Test_14_01(B foo)
        {
            foo.ExtMethodB();
        }

        public void Test_14_02(B foo)
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
namespace AppendedNamespaceForConcurrencyTest.Test_15
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

        public void Test_15(B foo) // Noncompliant
        {
            foo.SomeEvent += Foo_SomeEvent;
        }

        public void Test_15_01(A foo)
        {
            foo.SomeEvent += Foo_SomeEvent;
        }

        private void Foo_SomeEvent(object sender, EventArgs e) { }
    }
}

// Test multiple parameters
namespace AppendedNamespaceForConcurrencyTest.Test_16
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

        public void Test_16(B foo1, B foo2, B foo3)
//                            ^^^^ Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Test_16.A' instead of 'AppendedNamespaceForConcurrencyTest.Test_16.B'.}}
//                                    ^^^^ Noncompliant@-1 {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Test_16.A' instead of 'AppendedNamespaceForConcurrencyTest.Test_16.B'.}}
        {
            foo1.Method();
            var x = foo2.Property;
            foo3.OtherMethod();
        }
    }
}

// Test overridden method
namespace AppendedNamespaceForConcurrencyTest.Test_17
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
        public virtual void Test_17(B foo)
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
        public override void Test_17(B foo)
        {
            foo.Method();
        }
    }
}

// Test excluded types
namespace AppendedNamespaceForConcurrencyTest.Test_18
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

        public void Test_18_Object(B foo)
        {
            var x = foo.ToString();
        }

        public void Test_18_Enum(AreWeGood foo)
        {
            foo.HasFlag(AreWeGood.Yes);
        }

        public void Test_18_Interop(System.Type foo)
        {
            var x = foo.IsEnum;
        }

        public void Test_18_Unused(B foo)
        {
        }

        public void Test_18_Array(B[] foo)
        {
            var x = foo.Length;
        }

        public void Test_18_ValueType(Guid foo)
        {
            var x = foo.ToString();
        }

        public void Test_18_String(string foo)
        {
            var x = foo.Equals("");
        }
    }

    public class B : A { }
}

// Test parentheses
namespace AppendedNamespaceForConcurrencyTest.Test_19
{
    public class A
    {
        public void Method() { }

        public int Property { get; set; }
    }

    public class B : A
    {
        public void Test_19_Method(B foo) // Noncompliant
        {
            (((foo))).Method();
        }

        public void Test_19_Method2(B foo) // Noncompliant
        {
            Foo( (((foo))) );
        }

        public void Test_19_Property(B foo) // Noncompliant
        {
            (foo).Property = 1;
        }

        private void Foo(A thing) { }
    }
}

// Test unsupported
namespace AppendedNamespaceForConcurrencyTest.Test_20
{
    public class A
    {
        public void Method() { }
    }

    public class B : A
    {
        public void Test_19_Method(B foo) // False negative
        {
            (foo ?? null).Method();
        }
    }
}

// Test implementing interface
namespace AppendedNamespaceForConcurrencyTest.Test_21
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
namespace AppendedNamespaceForConcurrencyTest.Test_23
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
            list[0] = new Test_23.Foo();
        }
    }
}

// Test that rule doesn't suggest other types for EventHandler methods
namespace AppendedNamespaceForConcurrencyTest.Test_24
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

namespace AppendedNamespaceForConcurrencyTest.Something
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
namespace AppendedNamespaceForConcurrencyTest.Test_25
{
    public class Bar
    {
        public void MethodOne(Something.Foo f)
        {
            var x = f.IsFoo;
        }

        protected void MethodTwo(Something.Foo f)
        {
            var x = f.IsFoo;
        }

        private void MethodThree(Something.Foo f) // Noncompliant
        {
            var x = f.IsFoo;
        }

        internal void MethodFour(Something.Foo f) // Noncompliant
        {
            var x = f.IsFoo;
        }

        public void MethodFive(Something.Bar f) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Something.Foo' instead of 'AppendedNamespaceForConcurrencyTest.Something.Bar'.}}
        {
            var x = f.IsFoo;
        }

        protected void MethodSix(Something.Bar f) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Something.Foo' instead of 'AppendedNamespaceForConcurrencyTest.Something.Bar'.}}
        {
            var x = f.IsFoo;
        }

        private void MethodSeven(Something.Bar f) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Something.IFoo' instead of 'AppendedNamespaceForConcurrencyTest.Something.Bar'.}}
        {
            var x = f.IsFoo;
        }

        internal void MethodEight(Something.Bar f) // Noncompliant {{Consider using more general type 'AppendedNamespaceForConcurrencyTest.Something.IFoo' instead of 'AppendedNamespaceForConcurrencyTest.Something.Bar'.}}
        {
            var x = f.IsFoo;
        }
    }
}

// Do not suggest ICollection<KVP<T1, T2>> instead of Dictionary<T1, T2>
namespace AppendedNamespaceForConcurrencyTest.Test_26
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
namespace AppendedNamespaceForConcurrencyTest.Test_26
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

namespace AppendedNamespaceForConcurrencyTest.Test_27
{
    public sealed class ReproIssue2479
    {
        public void SomeMethod(IReadOnlyList<S3242DeconstructibleType> list) // Noncompliant FP, using IReadOnlyCollection gives compile error
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

namespace AppendedNamespaceForConcurrencyTest.MultiConditionalAccess
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
