using System;
using System.Collections.Generic;


namespace Tests.Diagnostics
{
    public interface IMyInterface
    { /* ... */ }

    public class Implementer : IMyInterface { }

    public interface IMyInterface2
    { /* ... */ }

    public interface IMyInterface4 : IMyInterface
    { /* ... */ }

    public interface IMyInterface3 : IMyInterface
    { /* ... */ }

    public class MyClass1
    { /* ... */ }
    public class MyClass2
    { /* ... */ }

    public class MyClass3 : MyClass2, IMyInterface
    { /* ... */ }

    public class MyClass4
    { /* ... */ }

    public class InvalidCastToInterface
    {
        public class Nested : MyClass4, IDisposable
        {
            public void Dispose() { }
        }

        static void Main()
        {
            var myclass1 = new MyClass1();
            var x = (IMyInterface)myclass1; // Noncompliant
//                   ^^^^^^^^^^^^
            x = myclass1 as IMyInterface;
            bool b = myclass1 is IMyInterface;

            var arr = new MyClass1[10];
            var arr2 = (IMyInterface[])arr;

            var myclass2 = new MyClass2();
            var y = (IMyInterface)myclass2;

            IMyInterface i = new MyClass3();
            var c = (IMyInterface2)i; // Compliant
            IMyInterface4 ii = null;
            var d = (IMyInterface2)i; // Compliant
            var e = (IMyInterface3)i;

            var o = (object)true;
            e = (IMyInterface3)o;

            var coll = (IEnumerable<int>)new List<int>();

            var z = (IDisposable)new MyClass4();

            var w = (IDisposable)(new Node());
        }
    }

    public class DerivedNode : MiddleNode, IDisposable
    {
        public void Dispose() { }
    }
    public class MiddleNode : Node
    {

    }
    public class Node
    { }

    public class MyClass
    {
        public double? D { get; set; } = 1.001;
    }

    public class NullableTest
    {
        public void Test1()
        {
            int? i1 = null;
            var ii = (int)i1; // Noncompliant {{Nullable is known to be empty, this cast throws an exception.}}
//                   ^^^^^^^
        }
        public void Test2()
        {
            int? i2 = 10;
            var ii = (int)i2;
        }
        public void Test3()
        {
            int? i3 = null;
            var d = (double)i3; // Noncompliant
        }
        public void Test4()
        {
            int? i4 = null;
            var n = (NullableTest)i4; // don't care, custom cast
        }
        public void Test5()
        {
            int? i5 = null;
            var d = (double?)i5; // Compliant as the resulting type allows null
        }
        public void Test6()
        {
            int? i6 = 42;
            var d = (double?)i6;
        }
        public void Test7()
        {
            int i7 = 42;
            var d = (double)i7;
        }

        public void TestMethod(object obj)
        {
            var a = obj as MyClass;
            var test = (ushort?)a?.D;
        }

        public static explicit operator NullableTest(int? i)
        {
            return null;
        }
    }

    interface IFoo { }
    interface IBar { }

    class Foo : IFoo { }
    class Bar : IBar { public Bar(string foo) { } }
    class FooBar : IFoo, IBar { }
    sealed class FinalBar : IBar { }

    class Other
    {
        public void Method<T>(T generic) where T : new()
        {
            IFoo ifoo = null;
            IBar ibar = null;
            Foo foo = null;
            Bar bar = null;
            FooBar foobar = null;
            FinalBar finalbar = null;
            object o = null;

            o = (IFoo)bar;  // Noncompliant
            o = (IFoo)ibar;
            o = (Foo)bar; // Compliant; causes compiler error // Error [CS0030] - invalid cast
            o = (Foo)ibar;
            o = (IFoo)finalbar; // Compliant; causes compiler error // Error [CS0030] - invalid cast
            o = (Bar)generic; // Compliant; causes compiler error // Error [CS0030] - invalid cast

            o = bar  as IFoo;
            o = ibar as IFoo;
            o = ibar as Foo;
            o = generic as Bar;

            o = bar  is IFoo;
            o = ibar is IFoo;
            o = bar  is Foo;
            o = ibar is Foo;
            o = finalbar is IFoo;
            o = generic is Bar;
        }
    }
}

namespace Tests.Diagnostics
{
    namespace CSharp8
    {
        public class NullCoalescenceAssignment
        {
            public void NullCoalescenceAssignment_Null()
            {
                int? i1 = null;
                i1 ??= (int)i1; // Noncompliant {{Nullable is known to be empty, this cast throws an exception.}}
            }

            public void NullCoalescenceAssignment_NotNull()
            {
                int? i1 = 1;
                i1 ??= (int)i1;
            }

            public void NullCoalescenceAssignmentResult_Null()
            {
                int? i = null;
                i ??= null;
                var r1 = (int)i; // Noncompliant
            }

            public void NullCoalescenceAssignmentResult_NotNull()
            {
                int? i = null;
                i ??= 1;
                var r1 = (int)i;
            }
        }

        public class SwitchExpressions
        {
            public void Nullable_In_Arm_Noncompliant()
            {
                int? i = null;
                int? result = i switch
                {
                    null => (int) i, // Noncompliant
                    _ => 0
                };
            }

            public int AlwaysNull_Noncompliant(int val)
            {
                int? result = val switch
                {
                    1 => null,
                    2 => null,
                    _ => null
                };
                return (int)result; // Noncompliant
            }

            public int AlwaysNonNull(int val)
            {
                int? result = val switch
                {
                    1 => -1,
                    2 => -2,
                     _ => -5
                };
                return (int) result;
            }

            public int NullableHasValue(int? val)
            {
                return val.HasValue switch
                {
                    true => (int)val,
                    _ => 0
                };
            }

            public int NullableNoValue(int? val)
            {
                return val.HasValue switch
                {
                    false => (int)val, // FN Switch expressions are not constrained (See #2949)
                    _ => 0
                };
            }
        }

        public interface IWithDefaultMembers
        {
            void NoncompliantDefaultInterfaceMethod()
            {
                int? i1 = null;
                var i2 = (int)i1; // Noncompliant
            }

            void CompliantDefaultInterfaceMethod()
            {
                int? i1 = 1;
                var i2 = (int)i1;
            }
        }

        public class LocalStaticFunctions
        {
            public void Method(object arg)
            {
                void LocalFunction()
                {
                    int? i1 = null;
                    var i2 = (int)i1; // Compliant - FN: local functions are not supported by the CFG
                }

                static void LocalStaticFunction()
                {
                    int? i1 = null;
                    var i2 = (int)i1; // Compliant - FN: local functions are not supported by the CFG
                }
            }
        }
    }
}
