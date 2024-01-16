using System;

namespace Tests.Diagnostics
{
    public class C1
    {
        public virtual void MyNotOverriddenMethod() { }
    }
    internal partial class Partial1Part //Noncompliant
//           ^^^^^^^
    {
        void Method() { }
    }
    partial struct PartialStruct //Noncompliant {{'partial' is gratuitous in this context.}}
    {
    }
    partial interface PartialInterface //Noncompliant
    {
    }

    internal partial class Partial2Part
    {
    }

    internal abstract partial class Partial2Part
    {
        public virtual void MyOverriddenMethod() { }
        public virtual int Prop { get; set; }
        public abstract int this[int counter] { get; }
        protected abstract event EventHandler<EventArgs> MyEvent;
        protected abstract event EventHandler<EventArgs> MyEvent2;
    }

    internal class Override : Partial2Part
    {
        public override void MyOverriddenMethod() { }

        public override int this[int counter]
        {
            get { return 0; }
        }

        protected override event EventHandler<EventArgs> MyEvent;
        protected override event EventHandler<EventArgs> MyEvent2
        {
            add { }
            remove { }
        }

        public enum SomeEnumeration
        {

        }
    }

    sealed class SealedClass : Partial2Part
    {
        public override sealed void MyOverriddenMethod() { } // Noncompliant {{'sealed' is redundant in this context.}}
//                      ^^^^^^
        public override sealed int Prop { get; set; } // Noncompliant

        public override sealed int this[int counter] // Noncompliant
        {
            get { return 0; }
        }

        protected override sealed event EventHandler<EventArgs> MyEvent; // Noncompliant
        protected override sealed event EventHandler<EventArgs> MyEvent2 // Noncompliant
        {
            add { }
            remove { }
        }
    }

    abstract class AbstractClass : Partial2Part
    {
        public override sealed void MyOverriddenMethod() { }
        public override sealed int Prop { get; set; }

        public override sealed int this[int counter]
        {
            get { return 0; }
        }

        protected override sealed event EventHandler<EventArgs> MyEvent;
        protected override sealed event EventHandler<EventArgs> MyEvent2
        {
            add { }
            remove { }
        }
    }

    sealed class SealedClassWithoutRedundantKeywordOnMembers : Partial2Part
    {
        public override void MyOverriddenMethod() { }

        public override int Prop { get; set; }

        public override int this[int counter]
        {
            get { return 0; }
        }

        protected override event EventHandler<EventArgs> MyEvent;
        protected override event EventHandler<EventArgs> MyEvent2
        {
            add { }
            remove { }
        }
    }

    internal class BaseClass<T>
    {
        public virtual string Process(string input)
        {
            return input;
        }
    }

    internal class SubClass : BaseClass<string>
    {
        public override string Process(string input)
        {
            return "Test";
        }
    }

    unsafe class UnsafeClass
    {
        int* pointer;
    }

    unsafe class UnsafeClass2 // Noncompliant
//  ^^^^^^
    {
        int num;
    }
    unsafe class UnsafeClass3 // Noncompliant {{'unsafe' is redundant in this context.}}
    {
        unsafe void M() // Noncompliant
//      ^^^^^^
        {

        }
    }

    class Point
    {
        public int x;
        public int y;
    }

    class Class4
    {
        unsafe interface MyInterface
        {
            unsafe int* Method(); // Noncompliant
        }

        private unsafe delegate void MyDelegate(int* p);
        private unsafe delegate void MyDelegate2(int i); // Noncompliant

        unsafe class Inner { } // Noncompliant

        unsafe event MyDelegate MyEvent; // Noncompliant
        unsafe event MyDelegate MyEvent2
        {
            add
            {
                int* p;
            }
            remove { }
        }

        unsafe ~Class4() // Noncompliant
        {
        }
        void M()
        {
            Point pt = new Point();
            unsafe
            {
                fixed (int* p = &pt.x)
                {
                    *p = 1;
                }
            }

            unsafe
            {
                unsafe // Noncompliant
                {
                    unsafe // Noncompliant
                    {
                        var i = 1;
                        int* p = &i;
                    }
                }
            }
        }
    }

    public class Foo
    {
        public class Bar
        {
            public unsafe class Baz // Noncompliant
            {
            }
        }
    }

    public unsafe class Foo2
    {
        public unsafe class Bar // Noncompliant
        {
            private int* p;

            public unsafe class Baz // Noncompliant
            {
                private int* p2;
            }
        }
    }

    public class Checked
    {
        public static void M()
        {
            checked
            {
                checked // Noncompliant
//              ^^^^^^^
                {
                    var z = 1 + 4;
                    var y = unchecked(1 +
                        unchecked(4)); // Noncompliant
//                      ^^^^^^^^^
                }
            }

            checked // Noncompliant {{'checked' is redundant in this context.}}
            {
                var f = 5.5;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5 + 4);
            }

            checked
            {
                var f = 5.5;
                var x = 5 + 4;
                var y = unchecked(5.5 + 4); // Noncompliant
            }

            checked // Noncompliant
            {
                var f = 5.5;
                var x = 5 + "somestring";
            }

            unchecked
            {
                var f = 5.5;
                var y = unchecked(5 + 4); // Noncompliant
            }

            checked
            {
                var x = (uint)10;
                var y = (int)x;
            }

            checked // Noncompliant
            {
                var x = 10;
                var y = (double)x;
            }

            checked
            {
                var x = 10;
                x += int.MaxValue;
            }

            checked
            {
                var x = 10;
                x = -int.MaxValue;
            }

            checked
            {
                var x = 10;
                x = -5;
            }

            checked // Noncompliant
            {
                var x = 10;
                x = -"1"; // Error [CS0023]
            }

            checked // Noncompliant
            {
                var x = 10;
                x = +5;
            }

            checked  // Noncompliant
            {
                var x = 10;
                var y = 5 % x;
            }

            checked
            {
                var x = (uint)null; // Error [CS0037]
                var y = (int)x;
            }

            checked
            {
                var x = (SomeClass)5; // Error [CS0246]
                var y = (int)x;
            }
        }
    }

    public unsafe struct FixedArraySample
    {
        private fixed int a[10];
    }

    public class StackAlloc
    {
        private unsafe void M()
        {
            var x = stackalloc int[100];
        }
    }

    public class UnsafeHidden
    {
        private unsafe int* Method() { return null; }
        private unsafe void Method2(int* p) { }
        public unsafe void M1()
        {
            Method();
        }

        public unsafe void WithUndefinedInvocation()    // Noncompliant
        {
            Undefined();        // Error [CS0103]: The name 'Undefined' does not exist in the current context
        }

        private unsafe int* Prop { get; set; }
        public unsafe void M2()
        {
            Method2(Prop);
        }
    }

    public class UnsafeParameter
    {
        public unsafe delegate void Unsafe(int* x);

        public unsafe void Method()
        {
            Unsafe u = (a) => { };
        }
    }

    public class UnsafeCtor
    {
        public UnsafeCtor()
        {
            unsafe // Noncompliant
            {
            }
        }
    }
}
